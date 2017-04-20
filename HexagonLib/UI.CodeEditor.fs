module Hexagon.UI.CodeEditor

open Hexagon
open Fable.Import.Browser

let mutable editor: Fable.Import.AceAjax.Editor = null

let initialize (container: HTMLElement) =
    editor <- Fable.Import.Globals.ace.edit(container)
    editor.getSession().setMode("ace/mode/javascript");
    editor.setValue("""(function(_) {
    // you can store state here

return function play(cells) {
    // For help, you can click on "?" button, top right. (input/output details and rules)
    
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
})""") |> ignore

let getValue compile : (Ais.AiCell[] -> Ais.TransactionParameters option) =
    editor.getValue() |> compile