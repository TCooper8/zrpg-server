module Test.Rand

open System

let rand = new Random()

let nextStr =
  let chars = "abcdefghijklmnopqrstuvwxyz1234567890ABCDEFGHIJKLMNOPQRSTUVWXYZ"
  let clen = chars.Length

  fun len ->
    let rchars = Array.init len (fun i -> chars.[rand.Next clen])
    new String(rchars)