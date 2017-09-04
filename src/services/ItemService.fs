module Zrpg.ItemService

open System
open Npgsql
open Zrpg.Items
open Zrpg.Types

type Service = {
  connect: unit -> NpgsqlConnection Async
}

let internal setupCmd () =
  """ create extension if not exists pgcrypto;

      create table if not exists item_types (
        id uuid primary key default gen_random_uuid(),
        label varchar(128) not null unique
      );

      create table if not exists items (
        id uuid primary key default gen_random_uuid(),
        item_type uuid not null references item_types(id)
      );

      create table if not exists weapons (
        item_id uuid not null references items(id),
        name varchar(128) not null,
        description text not null,
        damage int not null
      );

      create table if not exists armors (
        item_id uuid not null references items(id),
        name varchar(128) not null,
        description text not null,
        damage int not null
      );

      insert into item_types (label) values ('weapon') on conflict do nothing;
      insert into item_types (label) values ('armor') on conflict do nothing;
  """

  |> Sql.cmd

let setup service = async {
  use cmd = setupCmd ()
  use! conn = service.connect()
  do!
    cmd
    |> Sql.withConn conn
    |> Sql.asScalar
    |> Async.Ignore
  ()
}

let internal addWeaponCmd (weapon:Weapon) =
  """ with new_item as (
        insert into items (
          item_type
        )
        values (
          (select (id) from item_types where label='weapon')
        )
        returning id
      )
      insert into weapons (
        item_id,
        name,
        description,
        damage
      )
      values (
        (select id from new_item),
        :name,
        :description,
        :damage
      )
      returning (select id from new_item);
  """
  |> Sql.cmd
  |> Sql.withParams
    [ "name", weapon.name :> obj
      "description", weapon.description :> obj
      "damage", weapon.damage :> obj
    ]

let addWeapon service (weapon:Weapon): ItemId Reply Async = async {
  try
    use cmd = addWeaponCmd weapon
    use! conn = service.connect()
    let! id =
      cmd
      |> Sql.withConn conn
      |> Sql.asScalar

    let id = id :?> Guid
    return Created id
  with e ->
    return ServerError e
}

let internal addArmorCmd (armor:Armor) =
  """ with new_item as (
        insert into items (
          item_type
        )
        values (
          (select (id) from item_types where label='armor')
        )
        returning id
      )
      insert into armors (
        item_id,
        name,
        description,
        damage
      )
      values (
        (select id from new_item),
        :name,
        :description,
        :defense
      )
      returning (select id from new_item);
  """
  |> Sql.cmd
  |> Sql.withParams
    [ "name", armor.name :> obj
      "description", armor.description :> obj
      "defense", armor.defense :> obj
    ]

let addArmor service (armor:Armor): ItemId Reply Async = async {
  try
    use cmd = addArmorCmd armor
    use! conn = service.connect()
    let! id =
      cmd
      |> Sql.withConn conn
      |> Sql.asScalar

    let id = id :?> Guid
    return Created id
  with e ->
    return ServerError e
}

let internal getCmd (id:ItemId) =
  """ select
        item_type.label,
        weapon.*,
        armor.*
      from items as item
        inner join item_types as item_type
          on item_type.id=item.item_type
        left join weapons as weapon
          on weapon.item_id=item.id
        left join armors as armor
          on armor.item_id=item.id
      where item.id=:id
  """
  |> Sql.cmd
  |> Sql.withParam "id" id

let internal weaponOffset = 1
let internal armorOffset = 5

let get service (id:ItemId) = async {
  try
    use cmd = getCmd id
    use! conn = service.connect()
    let! reader =
      cmd
      |> Sql.withConn conn
      |> Sql.asReader

    let! read = reader.ReadAsync() |> Async.AwaitTask
    if not read then
      return NotFound "Item not found."
    else
      let fields = reader.FieldCount

      let! itemType = reader.GetFieldValueAsync(0) |> Async.AwaitTask
      match itemType with
      | "weapon" ->
        let! itemId = reader.GetFieldValueAsync(weaponOffset) |> Async.AwaitTask
        let! name = reader.GetFieldValueAsync(weaponOffset + 1) |> Async.AwaitTask
        let! description = reader.GetFieldValueAsync (weaponOffset + 2) |> Async.AwaitTask
        let! damage = reader.GetFieldValueAsync(weaponOffset + 3) |> Async.AwaitTask
        let weapon =
          { name = name
            description = description
            damage = damage
          }
        return
          Ok
            { id = itemId
              itemType = Weapon weapon
            }
      | "armor" ->
        let! itemId = reader.GetFieldValueAsync(armorOffset) |> Async.AwaitTask
        let! name = reader.GetFieldValueAsync(armorOffset + 1) |> Async.AwaitTask
        let! description = reader.GetFieldValueAsync(armorOffset + 2) |> Async.AwaitTask
        let! defense = reader.GetFieldValueAsync(armorOffset + 3) |> Async.AwaitTask
        let armor:Armor =
          { name = name
            description = description
            defense = defense
          }
        return
          Ok
            { id = itemId
              itemType = Armor armor
            }
      | _ ->
        return failwithf "Unhandled itemType '%s'" itemType
  with e ->
    return ServerError e
}