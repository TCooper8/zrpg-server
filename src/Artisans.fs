module Zrpg.Artisans

open System
open Zrpg.Zones
open Zrpg.Blacksmithing

type ArtisanType =
  | Blacksmith

type ArtisanId = Guid
type Artisan = {
  id: ArtisanId
  artisanType: ArtisanType
}