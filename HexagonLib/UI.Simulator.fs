module Hexagon.UI.Simulator

open Fable.Core
open Hexagon.Domain
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
    
let mutable isRunning = false
let mutable isPaused = false

let mutable nextStep: GameStep = End (Stopped, [])

let runNextStep () = 
    match nextStep with
    | NextRound (_, action) -> 
            nextStep <- action()
    | End (_, _) -> 
            isRunning <- false
            isPaused <- false

let rec play () =
    match isRunning, isPaused with
    | false, _ -> 
        nextStep <- End (Stopped, [])
    | _, true -> 
        ()
    | true, false -> 
        runNextStep()
        setTimeout 10 play

let startGame hexagonSize roundsNb ais = 
    nextStep <- Hexagon.Game.startGame handleMessage hexagonSize roundsNb ais

    play()

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
        match isPaused with
        | true -> 
            isPaused <- false
            play()
        | false -> 
            isRunning <- true
            isPaused <- false
        
            [
                ({ Id = 1; Name = "Basic JS" }, basicAiJs )
                ({ Id = 2; Name = "Basic F#" }, Hexagon.BasicAi.play )
                ({ Id = 3; Name = "Dynamic JS" }, getPlayFunction() )
            ]
            |> startGame 9 5000)
        
    let stopButton = document.getElementById("stop");
    addListenerOnClick stopButton (fun _ -> 
        isRunning <- false
        isPaused <- false)
        
    let pauseButton = document.getElementById("pause");
    addListenerOnClick pauseButton (fun _ -> 
        match isRunning with
        | false -> ()
        | true -> 
            isPaused <- not isPaused
            play())
