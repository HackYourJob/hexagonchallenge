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

[<Emit("""(function(code, _) {
    try {
        return eval($0)($1);
    } catch (e) {
        console.log(e);
        return function() {};
    }
})($0, $1);""")>]
let eval (code: string) underscore : (obj -> Ais.TransactionParameters option) = jsNative

[<Emit("""(function(aiName, play) {
    let nbTimeout = 0;
    return function(cells) {
        let timer = Date.now()

        if (nbTimeout < 10) {
            var result = play(cells)
        }

        let delay = Date.now() - timer
        if(delay > 50) {
            console.log("Timeout for " + aiName + ": " + delay)
            throw "Timeout"
        }
        if (nbTimeout >= 10) {
            console.log("Too much timeout," + aiName + " excluded from Game")
            throw "Too much timeout, excluded from Game";
        }

        return result;
    };
})($0, $1);""")>]
let checkExecutionDelay (aiName: string) (play: Ais.AiCell[] -> Ais.TransactionParameters option) : (Ais.AiCell[] -> Ais.TransactionParameters option) = jsNative

let compile (code: string, aiName: string): (Ais.AiCell[] -> Ais.TransactionParameters option) = 
    let play = eval code underscore

    checkExecutionDelay aiName (improveInput >> play >> checkOuput)
