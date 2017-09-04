module Zrpg.Zones

open System
open Zrpg.Garrisons

type ZoneId = Guid

type Zone = {
  id: ZoneId
  garrison: GarrisonId
  name: string
}