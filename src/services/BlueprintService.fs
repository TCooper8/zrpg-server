module Zrpg.BlueprintService

open System
open System.Text

open Npgsql
open Zrpg.Blueprints
open Zrpg.Types

type Service = {
  connect: unit -> NpgsqlConnection Async
}

let internal setupCmd () =
  """ create extension if not exists pgcrypto;

      create table if not exists blueprints (
        id uuid primary key default gen_random_uuid(),
        title varchar(128) not null unique,
        description text not null,
        required_level int not null
      );

      create table if not exists blueprint_components (
        blueprint_id uuid not null references blueprints(id) on delete cascade,
        item_id uuid not null references items(id) on delete cascade
      );

      create table if not exists blueprint_produces (
        blueprint_id uuid not null references blueprints(id) on delete cascade,
        item_id uuid not null references items(id) on delete cascade
      );

      create table if not exists blueprint_auto_leans (
        blueprint_id uuid not null references blueprints(id) on delete cascade,
        on_level int not null
      );
  """
  |> Sql.cmd

let setup service = async {
  use cmd = setupCmd()
  use! conn = service.connect()
  do!
    cmd
    |> Sql.withConn conn
    |> Sql.asScalar
    |> Async.Ignore
}

let addCmd blueprint =
  let query =
    """
      with new_blueprint as (
        insert into blueprints (
            title,
            description,
            required_level
        )
        values (
          :title,
          :description,
          :required_level
        )
        returning id
      )
    """

  let componentsQuery =
    """
      , components as (
        insert into blueprint_components
          (blueprint_id, item_id)
          values
            %blueprint_components%
      )
    """

  let producesQuery =
    """
      , produces as (
        insert into blueprint_produces
          (blueprint_id, item_id)
          values
            %blueprint_produces%
      )
    """

  query
  |> fun query ->
    if Seq.isEmpty blueprint.components then query
    else
      let builder = StringBuilder()
      blueprint.components
      |> Seq.mapi (fun i _ -> i)
      |> Seq.fold (fun (acc:StringBuilder) index ->
        //acc.AppendLine (componentQuery index)
        index
        |> sprintf
          """
            ((select id from new_blueprint), :component_id_%i)
          """
        |> acc.AppendLine
      ) builder

      componentsQuery.Replace("%blueprint_components%", string builder)
      |> (+) query
  |> fun query ->
    if Seq.isEmpty blueprint.produces then query
    else
      let builder = StringBuilder()

      blueprint.produces
      |> Seq.mapi (fun i _ -> i)
      |> Seq.fold (fun (acc:StringBuilder) index ->
        index
        |> sprintf
          """
            ((select id from new_blueprint), :produce_id_%i)
          """
        |> acc.AppendLine
      ) builder

      producesQuery.Replace("%blueprint_produces%", string builder)
      |> (+) query
  |> fun query -> query + "(select id from new_blueprint)"
  |> Sql.cmd
  |> Sql.withParams
    [ "title", blueprint.title :> obj
      "description", blueprint.description :> obj
      "required_level", blueprint.requiredLevel :> obj
    ]
  |> fun cmd ->
    blueprint.components
    |> Seq.mapi (fun i value -> i, value :> obj)
    |> Seq.fold (fun cmd (i, value) ->
      Sql.withParam (sprintf "component_id_%i" i) value cmd
    ) cmd
  |> fun cmd ->
    blueprint.produces
    |> Seq.mapi (fun i value -> i, value :> obj)
    |> Seq.fold (fun cmd (i, value) ->
      Sql.withParam (sprintf "produce_id_%i" i) value cmd
    ) cmd

let add service blueprint = async {
  try
    use cmd = addCmd blueprint
    use! conn = service.connect()
    let! id =
      cmd
      |> Sql.withConn conn
      |> Sql.asScalar

    let id = id :?> Guid
    return Created id
  with
  | :? System.AggregateException as e ->
    match e.InnerException with
    | :? PostgresException as e ->
      if e.SqlState = "23505" then
        let constraint = e.ConstraintName.Replace("blueprints_", "").Replace("_key", "")
        return Conflict (sprintf "Blueprint '%s' is not unique." constraint)
      else
        return ServerError e
    | e ->
      return ServerError e
  | e ->
    return ServerError e
}