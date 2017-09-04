module Zrpg.Artisans

open System
open Zrpg.Zones
open Zrpg.Blueprints

type ArtisanType =
  | Blacksmith

type ArtisanId = Guid
type Artisan = {
  id: ArtisanId
  artisanType: ArtisanType
}