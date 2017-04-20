module Hexagon.UI.CodeEditor

open Hexagon
open Fable.Import.Browser

let mutable editor: Fable.Import.AceAjax.Editor = null

let initialize (container: HTMLElement) =
    editor <- Fable.Import.Globals.ace.edit(container)
    editor.getSession().setMode("ace/mode/javascript");
    editor.setValue("""(function(_) {
return function(cells) {
    // cells is an array of cell you own { Id : 'some-id', Resources : 12, Neighbours : [...] }, where 
    // - Resources cannot be more than 100, it is the available resources to move from a cell
    // - Neighbours is an array of cell, with Id and Resources properties only
    // NOTE: you don't know who is the cell's owner, and by the way, Neighbours include your cells also
    
    // Your clever code here :)
    let maxResourcesDiff = 0;
        let selectedTuple;
        cells.forEach(function (c) {
            c.Neighbours.forEach(function (n) {
                let resourcesDiff = c.Resources - n.Resources;
                if (n.Owner !== "Own" && resourcesDiff > maxResourcesDiff) {
                    maxResourcesDiff = resourcesDiff;
                    selectedTuple = [c, n];
                }
            });
        });
        return { FromId: selectedTuple[0].Id, ToId: selectedTuple[1].Id, AmountToTransfer: selectedTuple[0].Resources };
    };
};
})""") |> ignore

let getValue compile : (Ais.AiCell[] -> Ais.TransactionParameters option) =
    editor.getValue() |> compile