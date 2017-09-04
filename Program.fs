open System

open Zrpg
open Zrpg.Primitives
open Zrpg.Types

[<EntryPoint>]
let main argv =
  let connectionString = "Server=localhost;User Id=postgres;Database=postgres;Port=5432"
  use conn = Sql.connect connectionString |> Async.RunSynchronously
  let items: ItemService.Service =
    { connect = fun () -> Sql.connect connectionString
    }

  let blueprints: BlueprintService.Service =
    { connect = fun () -> Sql.connect connectionString
    }

  0