module Test.Unit.ItemsTest

open Test
open Xunit

open Zrpg
open Zrpg.Items
open Zrpg.Types

let (|IsOk|_|) = function
  | Ok value -> Some value
  | otherwise ->
    Assert.True(false, sprintf "Expected Created(...), got %A" otherwise)
    None

let (|IsCreated|_|) = function
  | Created value -> Some value
  | otherwise ->
    Assert.True(false, sprintf "Expected Created(...), got %A" otherwise)
    None

let withService action = async {
  let connectionString = "Server=localhost;User Id=postgres;Database=postgres;Port=5432"
  use conn = Sql.connect connectionString |> Async.RunSynchronously
  let itemService: ItemService.Service =
    { connect = fun () -> Sql.connect connectionString
    }
  return! action itemService
}

let addArmor armor =
  let armor: Armor =
    defaultArg armor
      { name = Rand.nextStr 16
        description = Rand.nextStr 64
        defense = Rand.rand.Next 50
      }

  withService (fun service -> async {
    return! ItemService.addArmor service armor
  })

let addWeapon weapon =
  let weapon: Weapon =
    defaultArg weapon
      { name = Rand.nextStr 16
        description = Rand.nextStr 64
        damage = Rand.rand.Next 50
      }

  withService (fun service -> async {
    return! ItemService.addWeapon service weapon
  })

[<Fact>]
let ``Should setup item tables.`` () =
  withService (fun service -> async {
    do! ItemService.setup service
  })

[<Fact>]
let ``Should add a piece of armor.`` () = async {
  let! (IsCreated id) = addArmor None
  printfn "Created %A" id
}

[<Fact>]
let ``Should add a piece of weapon.`` () = async {
  let! (IsCreated id) = addWeapon None
  printfn "Created %A" id
}