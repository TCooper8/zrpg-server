module Test.Utils

open System
open Xunit

open Zrpg.Types

let (|IsOk|_|): 'a Reply -> 'a option = function
  | Ok value -> Some value
  | otherwise ->
    Assert.True(false, sprintf "Expected Ok(...), got %A" otherwise)
    None

let (|IsCreated|_|): 'a Reply -> 'a option = function
  | Created value -> Some value
  | otherwise ->
    Assert.True(false, sprintf "Expected Created(...), got %A" otherwise)
    None