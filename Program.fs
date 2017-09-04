open Zrpg
open Zrpg.Types

[<EntryPoint>]
let main argv =
  let connectionString = "Server=localhost;User Id=postgres;Database=postgres;Port=5432"
  use conn = Sql.connect connectionString |> Async.RunSynchronously
  let itemService: ItemService.Service =
    { connect = fun () -> Sql.connect connectionString
    }

  async {
    do! ItemService.setup itemService
    printfn "Setup tables"
    let! reply =
      ItemService.addArmor itemService
        { name = "Helmet"
          description = "None."
          //damage = 50
          defense = 50
        }
    let (Created id) = reply
    printfn "Added axe %A" id
    let! item = ItemService.get itemService id
    printfn "Got axe %A" item
  }
  |> Async.RunSynchronously

  0
