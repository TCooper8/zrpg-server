module Test.Unit.BlueprintsTest

open System

open Test
open Test.Utils
open Xunit

open Zrpg
open Zrpg.Items
open Zrpg.Blueprints
open Zrpg.Types
open Zrpg.Primitives

let connect =
  let connectionString = "Server=localhost;User Id=postgres;Database=postgres;Port=5432"
  fun () -> Sql.connect connectionString

let withService action = async {
  let service: BlueprintService.Service =
    { connect = connect
    }
  return! action service
}

let withItems action =
  let service: ItemService.Service =
    { connect = connect
    }
  action service

let addWeapon weapon =
  let weapon: Weapon =
    defaultArg weapon
      { name = Rand.nextStr 16
        description = Rand.nextStr 64
        damage = Rand.rand.Next 50
      }

  withItems (fun items -> async {
    return! ItemService.addWeapon items weapon
  })

let addBlueprint blueprint =
  let blueprint =
    defaultArg blueprint
      { id = Guid.Empty
        title = Rand.nextStr 16
        description = Rand.nextStr 64
        requiredLevel = Rand.rand.Next 255 * 1<Level>
        components = Seq.empty
        produces = Seq.empty
      }

  withService (fun service ->
    BlueprintService.add service blueprint
  )

let setup = async {
  do!
    withItems (fun items ->
      ItemService.setup items
    )
  do!
    withService (fun service ->
      BlueprintService.setup service
    )
}

[<Fact>]
let ``Should be able to create a blueprint.`` () = async {
  do! setup
  let! (IsCreated itemId) = addWeapon None
  let! (IsCreated id) =
    let blueprint: Blueprint =
      { id = Guid.Empty
        title = Rand.nextStr 16
        description = Rand.nextStr 64
        requiredLevel = Rand.rand.Next 255 * 1<Level>
        components = [ itemId ]
        produces = [ itemId ]
      }
    addBlueprint (Some blueprint)
  ()
}