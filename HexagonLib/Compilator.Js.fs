module Hexagon.Compilator.Js

open Hexagon
open Fable.Core

[<Emit("eval($0)()")>]
let compile (code: string): (Ais.AiCell[] -> Ais.TransactionParameters option) = jsNative
