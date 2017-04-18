module Hexagon.UI.CodeEditor

open Hexagon
open Fable.Import.Browser

let mutable editor: Fable.Import.AceAjax.Editor = null

let initialize (container: HTMLElement) =
    editor <- Fable.Import.Globals.ace.edit(container)
    editor.getSession().setMode("ace/mode/javascript");
    editor.setValue("""(function(_) {
return function(cells) {
    // cells if an array of cell you own { Id : 'some-id', Resources : 12, Neighbours : [...] }, where 
    // - Resources cannot be more than 100, it is the available resources to move from a cell
    // - Neighbours is an array of cell, with Id and Resources properties only
    // NOTE: you don't know who is the cell's owner, and by the way, Neighbours include your cells also
    
    // Your clever code here :)
    
    if (cells[cells.length - 1].Resources > 10 
        && cells[cells.length - 1].Resources > cells[0].Neighbours[0].Resources) {
        return { 
            FromId: cells[cells.length - 1].Id, 
            ToId: cells[0].Neighbours[0].Id, 
            AmountToTransfer: cells[cells.length - 1].Resources - 1 
        };
    }
};
})""") |> ignore

let getValue compile : (Ais.AiCell[] -> Ais.TransactionParameters option) =
    editor.getValue() |> compile