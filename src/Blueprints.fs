module Zrpg.Blacksmithing

open System

open Zrpg.Primitives
open Zrpg.Items

type BlueprintId = Guid
type Blueprint = {
  id: BlueprintId
  title: string
  description: string
  requiredLevel: int<Level>
  components: ItemId seq
  produces: ItemId seq
}

type BlueprintAutoLearn = {
  blueprint: BlueprintId
  onLevel: int<Level>
}