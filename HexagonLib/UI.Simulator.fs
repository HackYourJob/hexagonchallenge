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
    
let mutable isCancelled = false

let startGame hexagonSize roundsNb ais = 
    let rec deferNextStep nextStep = 
        match isCancelled, nextStep with
        | true, _ -> ()
        | false, NextRound (num, action) -> 
                setTimeout 10 (fun () -> action() |> deferNextStep)
        | false, End (reason, score) -> ()

    let nextStep = Hexagon.Game.startGame handleMessage hexagonSize roundsNb ais

    deferNextStep nextStep

let addListenerOnClick (button: HTMLElement) action =
    button.addEventListener_click(fun _ -> 
        action()
        new obj())

let initializeAiSimulator basicAiJs =
    CodeEditor.initialize (document.getElementById("code"))
    Legend.initialize (document.getElementById("scores"))
    Board.initialize (document.getElementById("board"))

    let testButton = document.getElementById("test");
    addListenerOnClick testButton (fun _ -> 
        isCancelled <- false
        
        [
            ({ Id = 1; Name = "Basic JS" }, basicAiJs )
            ({ Id = 2; Name = "Basic F#" }, play )
            ({ Id = 3; Name = "Dynamic JS" }, getPlayFunction() )
        ]
        |> startGame 9 5000)
        
    let stopButton = document.getElementById("stop");
    addListenerOnClick stopButton (fun _ -> isCancelled <- true)
