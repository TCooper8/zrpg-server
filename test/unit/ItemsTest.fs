module Test.Unit.ItemsTest

open Test
open Test.Utils
open Xunit

open Zrpg
open Zrpg.Items
open Zrpg.Types

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
      { name = Rand.nextStr 64
        description = Rand.nextStr 64
        defense = Rand.rand.Next 50
      }

  withService (fun service -> async {
    return! ItemService.addArmor service armor
  })

let getItem id =
  withService (fun service ->
    ItemService.get service id
  )

let addWeapon weapon =
  let weapon: Weapon =
    defaultArg weapon
      { name = Rand.nextStr 64
        description = Rand.nextStr 64
        damage = Rand.rand.Next 50
      }

  withService (fun service -> async {
    return! ItemService.addWeapon service weapon
  })

let setup = async {
  do!
    withService (fun service ->
      ItemService.setup service
    )
}

[<Fact>]
let ``Should setup item tables.`` () = setup

[<Fact>]
let ``Should add a piece of armor.`` () = async {
  do! setup
  let! (IsCreated id) = addArmor None
  printfn "Created %A" id
}

[<Fact>]
let ``Should be able to get armor that has been added.`` () = async {
  do! setup
  let expected: Armor =
    { name = Rand.nextStr 64
      description = Rand.nextStr 64
      defense = Rand.rand.Next 50
    }

  let! (IsCreated id) = addArmor (Some expected)
  let! (Ok { id = id; itemType = Armor armor }) = getItem id
  Assert.True(compare armor expected = 0, sprintf "Expected %A but got %A" expected armor)
}

[<Fact>]
let ``Should add a piece of weapon.`` () = async {
  do! setup
  let! (IsCreated id) = addWeapon None
  printfn "Created %A" id
}

[<Fact>]
let ``Should be able to get weapon that has been added.`` () = async {
  do! setup
  let expected: Weapon =
    { name = Rand.nextStr 64
      description = Rand.nextStr 64
      damage = Rand.rand.Next 50
    }

  let! (IsCreated id) = addWeapon (Some expected)
  let! (IsOk { id = id; itemType = Weapon weapon }) = getItem id
  Assert.True(compare weapon expected = 0, sprintf "Expected %A but got %A" expected weapon)
}