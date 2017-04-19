module Hexagon.TournamentWorker

open Fable.Import.Node
open Hexagon.Domain
open Fable.Core

let hexagonSize = 9
let roundsNb = 5000

[<Emit("console.log($0)")>]
let printLog (value: obj) = jsNative

let handleMessage evt =
    printLog evt

let rec runNextStep nextStep = 
    match nextStep with
    | NextRound (_, action) -> 
            action() |> runNextStep
    | End (_, _) -> ()

let startGame ais = 
    let nextStep = Game.startGame handleMessage hexagonSize roundsNb ais

    runNextStep nextStep

let play () =
    [
        ({ Id = 1; Name = "Basic F#" }, Hexagon.BasicAi.play )
    ]
    |> startGame


