module Zrpg.Types

open System
open Zrpg.Garrisons
open Zrpg.Auths

type 'a Reply =
  | Ok of 'a
  | Created of Guid
  | NotFound of string
  | Conflict of string
  | ServerError of exn