module Hexagon.Game

open Domain
open Hexagon
open Hexagon.Shapes

let startGame raiseEvents hexagonSize roundsNb ais : GameStep =
    let nbPlayers = ais |> List.length
    let basicAis = 
        [nbPlayers + 1..6]
        |> List.map (fun i -> { Id = i; Name = sprintf "Basic AI %i" i }, Hexagon.BasicAi.play)
    let rand = fun () -> (new System.Random()).Next()
    let ais = 
        ais @ basicAis
        |> List.map (fun ai -> ai, rand())
        |> List.sortBy snd
        |> List.map fst
    
    let hexagon = 
        HexagonBoard.generate hexagonSize 
        |> convertShapeToCells
        |> Seq.toList

    let board = CellsStore.CellsStore(hexagon, HexagonCell.isNeighbours)
    let aiCellsIdsStore = AiCellIdsStore.Store(hexagon)
    let scoresStore = ScoresStore.Store()
    
    let publishEvent evt =
        match evt with
        | Board (_, cellEvents, scoreEvents) -> 
            cellEvents |> Seq.iter board.apply
            scoreEvents |> Seq.iter scoresStore.apply
        | _ -> ()

        evt
        |> raiseEvents

    Started { 
        BoardSize = 
            { 
                Lines = hexagon |> Seq.map (fun c -> c.Id.LineNum) |> Seq.max 
                Columns = hexagon |> Seq.map (fun c -> c.Id.ColumnNum) |> Seq.max 
            } 
        Board = hexagon
        Ais = ais |> List.map fst }
    |> publishEvent
    
    ais
    |> Seq.zip (hexagon |> Seq.filter (fun c -> c.IsStartingPosition) |> Seq.map (fun c -> c.Id))
    |> Seq.map (fun (cell, (ai, _)) -> 
        AiAdded ai,
        [Owned { AiId = ai.Id; CellId = cell; Resources = 1 }], 
        [TerritoryChanged { AiId = ai.Id; ResourcesIncrement = 1; CellsIncrement = 1}])
    |> Seq.map Board
    |> Seq.iter publishEvent
        
    let wrapAiPlay = Ais.AntiCorruptionLayer.wrap aiCellsIdsStore.convertToAiCellId aiCellsIdsStore.convertToCellId
    let round = Round.createRunRound board.getCellsWithNeighboursOf board.getCell board.isNeighboursOf board.getAllOwnCells

    let rec runRound (nb: RoundNumber) ais = 
        round ais |> Seq.iter publishEvent
        
        match scoresStore.tryToGetWinner(), nb = roundsNb with
        | Some aiId, _ -> 
            GameEvents.Won aiId |> publishEvent
            End (GameEndReason.AiWon, scoresStore.getScores())
        | _, true -> 
            End (GameEndReason.RoundsNumberLimit, scoresStore.getScores())
        | _, false ->
            let nextRoundId = nb + 1
            NextRound (nextRoundId, (fun () -> runRound nextRoundId ais))

    ais 
    |> List.map (fun (ai, play) -> (ai.Id, wrapAiPlay play)) 
    |> runRound 1


