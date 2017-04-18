module Hexagon.UI.Simulator

open Fable.Core
open Hexagon.Domain
open Hexagon.BasicAi
open Fable.Import.Browser

[<Emit("setTimeout($1, $0)")>]
let setTimeout (delayInMs: int) (action: unit -> unit) = jsNative

[<Emit("console.log($0)")>]
let printLog (value: obj) = jsNative

let handleMessage evt = 
    Legend.apply evt 
    Board.apply evt
    printLog evt
    
let getPlayFunction () =
    CodeEditor.getValue Hexagon.Compilator.Js.compile

let startGame hexagonSize ais roundsNb isCancelled = 
    let rec deferNextStep nextStep = 
        match isCancelled(), nextStep with
        | true, _ -> ()
        | false, NextRound (num, action) -> 
                setTimeout 10 (fun () -> action() |> deferNextStep)
        | false, End (reason, score) -> ()

    let nextStep = Hexagon.Game.startGame handleMessage hexagonSize roundsNb ais

    deferNextStep nextStep

let initializeAiSimulator () =
    CodeEditor.initialize (document.getElementById("code"))
    Legend.initialize()