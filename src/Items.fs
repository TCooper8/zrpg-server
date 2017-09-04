module Zrpg.Items

open System

type Armor = {
  name: string
  description: string
  defense: int
}

type Weapon = {
  name: string
  description: string
  damage: int
}

type ItemType =
  | Weapon of Weapon
  | Armor of Armor

type ItemId = Guid
type Item = {
  id: ItemId
  itemType: ItemType
}