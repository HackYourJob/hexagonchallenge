module Hexagon.Compilator.Js

open Hexagon
open Fable.Core
open Fable.Core.JsInterop

let underscore = importAll<obj> "underscore"

[<Emit("""(function(cells) {
    cells.map(c => c.Neighbours.map(n => n.Owner = n.Owner.Case));
    return cells;
})($0);""")>]
let improveInput (cells: Ais.AiCell[]): obj = jsNative

[<Emit("""(function(result) {
    if(result == null || result == undefined) return undefined;

    if(result.FromId === undefined) throw "Missing FromId";
    if(result.ToId === undefined) throw "Missing FromId";
    if(result.AmountToTransfer === undefined) throw "Missing FromId";

    return { FromId: "" + result.FromId, ToId: "" + result.ToId, AmountToTransfer: + result.AmountToTransfer };
})($0);""")>]
let checkOuput (result: Ais.TransactionParameters option): Ais.TransactionParameters option = jsNative

[<Emit("eval($0)($1)")>]
let eval (code: string) underscore : (obj -> Ais.TransactionParameters option) = jsNative

let compile (code: string): (Ais.AiCell[] -> Ais.TransactionParameters option) = 
    let play = eval code underscore

    improveInput >> play >> checkOuput
