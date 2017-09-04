module Zrpg.Sql

open Npgsql

let connect connectionString = async {
  let conn = new NpgsqlConnection(connectionString)
  do! conn.OpenAsync() |> Async.AwaitTask
  return conn
}

let cmd sql =
  new NpgsqlCommand(sql)

let withParam (key:string) (value:obj) (cmd:NpgsqlCommand) =
  do cmd.Parameters.AddWithValue(key, value) |> ignore
  cmd

let withParams (pairs: (string * obj) seq) (cmd:NpgsqlCommand) =
  pairs
  |> Seq.fold (fun cmd (key, value) -> withParam key value cmd) cmd

let withConn conn (cmd:NpgsqlCommand) =
  cmd.Connection <- conn
  cmd

let asReader (cmd:NpgsqlCommand) =
  cmd.ExecuteReaderAsync()
  |> Async.AwaitTask

let asScalar (cmd:NpgsqlCommand) =
  cmd.ExecuteScalarAsync()
  |> Async.AwaitTask