module Hexagon.UI.Tournament

open System
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

let getSpeed () =
    let speedSelector = document.getElementById("speed") :?> HTMLSelectElement
    let selectedOption = speedSelector.options.[int speedSelector.selectedIndex] :?> HTMLOptionElement
    selectedOption.value |> int
    
let mutable isRunning = false
let mutable isPaused = false
let mutable speed = 100

let mutable events: GameEvents[] = [||]
let mutable currentEventNum = 0

let runNextStep () = 
    match currentEventNum < events.Length with
    | true -> 
        events.[currentEventNum] |> handleMessage
        currentEventNum <- currentEventNum + 1
    | false -> 
        isRunning <- false
        isPaused <- false

let rec autoPlay () =
    match isRunning, isPaused, speed with
    | false, _, _
    | _, true, _ 
    | true, false, -1 -> 
        ()
    | true, false, _ -> 
        runNextStep()
        setTimeout speed autoPlay

let play getEvents cleanEvents =
    speed <- getSpeed ()

    match isPaused with
    | true -> 
        isPaused <- false
        autoPlay()
    | false -> 
        isRunning <- true
        isPaused <- false
        currentEventNum <- 0
        
        events <- 
            getEvents () 
            |> Fable.Core.JsInterop.ofJson<GameEvents seq>
            |> cleanEvents
            |> Seq.toArray
        
        autoPlay()


let pause () =
    match isRunning with
    | false -> ()
    | true -> 
        isPaused <- not isPaused
        autoPlay()

let stop () =
    isRunning <- false
    isPaused <- false
    
let addListenerOnClick (button: HTMLElement) action =
    button.addEventListener_click(fun _ -> 
        action()
        new obj())

let initialize getEvents cleanEvents =
    Legend.initialize (document.getElementById("scores"))
    Board.initialize (document.getElementById("board"))

    let testButton = document.getElementById("test");
    addListenerOnClick testButton (fun _ -> play getEvents cleanEvents)
        
    let stopButton = document.getElementById("stop");
    addListenerOnClick stopButton stop
        
    let pauseButton = document.getElementById("pause");
    addListenerOnClick pauseButton pause
        
    document.addEventListener_keydown (fun evt -> 
        match evt.keyCode with
        | 32.0 -> pause()
        | 39.0 -> runNextStep()
        | _ -> ()

        new obj())
    