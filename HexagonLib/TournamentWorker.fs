module Hexagon.TournamentWorker

open Fable.Import.Node
open Hexagon.Domain
open Fable.Core

let hexagonSize = 9
let roundsNb = 100

[<Emit("console.log($0)")>]
let printLog (value: obj) = jsNative

let handleMessage (events: System.Collections.Generic.List<GameEvents>) (scoresStore: ScoresStore.Store) (evt: GameEvents) =
    match evt with
    | Board (_, cellEvents, scoreEvents) -> 
        scoreEvents |> Seq.iter scoresStore.apply
    | _ -> ()
    
    events.Add(evt)

let rec runNextStep nextStep = 
    match nextStep with
    | NextRound (_, action) -> 
            action() |> runNextStep
    | End (_, _) -> ()

type AiWorker = { code: string; order: int; name: string }

let play (ais: AiWorker seq) =
    let events = new System.Collections.Generic.List<GameEvents>()
    let scoresStore = ScoresStore.Store()

    ais
    |> Seq.sortBy (fun ai -> ai.order)
    |> Seq.map (fun ai -> ({ Id = ai.order; Name = ai.name }, (ai.code, ai.name) |> Compilator.Js.compile ))
    |> Seq.toList
    |> Game.startGame (handleMessage events scoresStore) hexagonSize roundsNb
    |> runNextStep

    events, scoresStore.getScores() |> Seq.sortByDescending (fun (id, v) -> v.CellsNb, v.Resources, v.BugsNb) |> Seq.toArray

open Fable.Core.JsInterop

let serializeEvents (events: GameEvents seq) = 
    toJson events
    
