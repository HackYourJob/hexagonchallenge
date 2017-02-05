module Hexagon.Game

open Domain
open Hexagon
open Hexagon.Shapes

let startGame raiseEvents (cancellationToken: System.Threading.CancellationToken) hexagonSize =
    let ais = [ ({ Id = 1; Name = "Basic1" }, BasicAi.play ); ({ Id = 2; Name = "Basic2" }, BasicAi.play )]
    let hexagon = 
        HexagonBoard.generate hexagonSize 
        |> convertShapeToCells
        |> Seq.toList

    let board = CellsStore.CellsStore(hexagon, HexagonCell.isNeighbours)
    let aiCellsIdsStore = AiCellIdsStore.Store(hexagon)
    
    let publishEvent evt =
        match evt with
        | Board (_, cellEvents) -> cellEvents |> Seq.iter board.apply
        | _ -> ()

        evt
        |> raiseEvents

    Started { BoardSize = { Lines = hexagon |> Seq.map (fun c -> c.Id.LineNum) |> Seq.max; Columns = hexagon |> Seq.map (fun c -> c.Id.ColumnNum) |> Seq.max }; Board = hexagon; Ais = ais |> Seq.map fst }
    |> publishEvent
    
    [
        1, hexagon |> Seq.head |> (fun c -> c.Id)
        2, hexagon |> Seq.rev |> Seq.head |> (fun c -> c.Id)
    ] 
    |> Seq.map (fun (ai, cell) -> AiAdded { AiId = ai; CellId = cell; Resources = 1 }, [Owned { AiId = ai; CellId = cell; Resources = 1 }])
    |> Seq.map Board
    |> Seq.iter publishEvent
        
    let wrapAiPlay = Ais.AntiCorruptionLayer.wrap aiCellsIdsStore.convertToAiCellId aiCellsIdsStore.convertToCellId
    let round = Round.createRunRound board.getCellsWithNeighboursOf board.getCell board.isNeighboursOf board.getAllOwnCells

    let rec runRound (nb: int) ais = 
        round ais |> Seq.iter publishEvent
        System.Console.WriteLine ("Round " + nb.ToString())

        if cancellationToken.IsCancellationRequested |> not
        then runRound (nb + 1) ais

    ais 
    |> List.map (fun (ai, play) -> (ai.Id, wrapAiPlay play)) 
    |> runRound 0
