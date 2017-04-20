module Hexagon.UI.Simulator

open System
open System.Drawing
open Fable.Core
open Hexagon.Domain
open Fable.Import.Browser
open Fable.Import.Fetch
open Fable.Helpers.Fetch

type Ai = {
   AiName : string
   UserId : string
   Password : string
   Content : string
}

[<Emit("setTimeout($1, $0)")>]
let setTimeout (delayInMs: int) (action: unit -> unit) = jsNative

[<Emit("console.log($0)")>]
let printLog (value: obj) = jsNative

let handleMessage evt = 
    Legend.apply evt 
    Board.apply evt
    printLog evt

    match evt with
    | AiPlayed (Bug bug) -> 
        let container = document.getElementById("bugLogs")
        container.innerText <- bug + "\n" + container.innerText
        ()
    | _ -> ()
    
let getPlayFunction () =
    CodeEditor.getValue Hexagon.Compilator.Js.compile

let getSpeed () =
    let speedSelector = document.getElementById("speed") :?> HTMLSelectElement
    let selectedOption = speedSelector.options.[int speedSelector.selectedIndex] :?> HTMLOptionElement
    selectedOption.value |> int
    
let mutable isRunning = false
let mutable isPaused = false
let mutable speed = 100

let mutable nextStep: GameStep = End (Stopped, [])

let runNextStep () = 
    match nextStep with
    | NextRound (_, action) -> 
            nextStep <- action()
    | End (_, _) -> 
            isRunning <- false
            isPaused <- false

let rec autoPlay () =
    match isRunning, isPaused, speed with
    | false, _, _ -> 
        nextStep <- End (Stopped, [])
    | _, true, _ 
    | true, false, -1 -> 
        ()
    | true, false, _ -> 
        runNextStep()
        setTimeout speed autoPlay

let startGame hexagonSize roundsNb ais = 
    nextStep <- Hexagon.Game.startGame handleMessage hexagonSize roundsNb ais

    autoPlay()

let play basicAiJs =
    speed <- getSpeed ()

    match isPaused with
    | true -> 
        isPaused <- false
        autoPlay()
    | false -> 
        isRunning <- true
        isPaused <- false

        let nameInput = document.getElementById("aiName") :?> HTMLInputElement
        
        [
            ({ Id = 1; Name = nameInput.value }, getPlayFunction() )
            ({ Id = 2; Name = "Basic JS" }, basicAiJs )
            ({ Id = 3; Name = "Basic F#" }, Hexagon.BasicAi.play )
        ]
        |> startGame 9 5000

let pause () =
    match isRunning with
    | false -> ()
    | true -> 
        isPaused <- not isPaused
        autoPlay()

let stop () =
    isRunning <- false
    isPaused <- false

let aiName () =
    document.getElementById("aiName").textContent

let userId () =
    document.getElementById("userId").textContent

let password () =
    document.getElementById("password").textContent

let createAiFromValues () =
    {
        AiName = aiName();
        UserId = userId();
        Password = password();
        Content = CodeEditor.editor.getValue()
    }

let onSomeSubmitResult result =
    window.alert("AI Saved " + result)|> ignore

let submit ()=   
    async {
        let! response = 
            postRecord(
                "http://localhost:8080/ais", 
                createAiFromValues(),
                [ Headers [ 
                    Accept "application/xml" ]
                ])
        if response.Ok then
            match response.Headers.ContentType with
            | None ->  window.alert("An error occured while saving AI") |> ignore
            | Some contentType -> onSomeSubmitResult contentType
    }

let addListenerAsyncOnClick (button: HTMLElement) action =
    button.addEventListener_click(fun _ ->
        action() |> Async.StartImmediate
        new obj())

let addListenerOnClick (button: HTMLElement) action =
    button.addEventListener_click(fun _ -> 
        action()
        new obj())

let initializeAiSimulator basicAiJs =
    CodeEditor.initialize (document.getElementById("code"))
    Legend.initialize (document.getElementById("scores"))
    Board.initialize (document.getElementById("board"))

    let testButton = document.getElementById("test");
    addListenerOnClick testButton (fun _ -> play basicAiJs)
        
    let stopButton = document.getElementById("stop");
    addListenerOnClick stopButton stop
        
    let pauseButton = document.getElementById("pause");
    addListenerOnClick pauseButton pause
        
    let hideRulesButton = document.getElementById("hideRulesButton");
    addListenerOnClick hideRulesButton (fun _ -> 
        let container = document.getElementById("rules") :?> HTMLDivElement
        container.style.visibility <- "collapse")
        
    let showRulesButton = document.getElementById("showRulesButton");
    addListenerOnClick showRulesButton (fun _ -> 
        let container = document.getElementById("rules") :?> HTMLDivElement
        container.style.visibility <- "visible")

    //remplacer par le JS pour le moment
    //let submitButton = document.getElementById("submit");
    //addListenerAsyncOnClick submitButton submit

    document.addEventListener_keydown (fun evt -> 
        match evt.keyCode with
        | 32.0 -> pause()
        | 39.0 -> runNextStep()
        | _ -> ()

        new obj())