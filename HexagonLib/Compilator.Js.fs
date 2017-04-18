module Hexagon.Compilator.Js

open Hexagon
open Fable.Core

[<Emit("""let func = eval($0)();
return cells => {
    let result = func(cells);
    if(result == null || result == undefined) return undefined;

    if(result.FromId === undefined) throw "Missing FromId";
    if(result.ToId === undefined) throw "Missing FromId";
    if(result.AmountToTransfer === undefined) throw "Missing FromId";

    return { FromId: "" + result.FromId, ToId: "" + result.ToId, AmountToTransfer: + result.AmountToTransfer };
}""")>]
let compile (code: string): (Ais.AiCell[] -> Ais.TransactionParameters option) = jsNative
