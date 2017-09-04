module Zrpg.Garrisons

open System

type GarrisonId = Guid

type Garrison = {
  id: GarrisonId
  title: string
}

type GarrisonCmd =
  | Get of GarrisonId