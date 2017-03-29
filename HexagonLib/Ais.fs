module Hexagon.Ais

open Domain

type AiCellId = string
type AiCell = {
        Id: AiCellId
        Resources: int
        Neighbours: NeighbourCell array
    }
and NeighbourCell = {
        Id: AiCellId
        Owner: CellOwner
        Resources: int
    }
and CellOwner = 
    | Own
    | Other
    | None
    
type Transaction = 
    | Move of TransactionParameters
    | Sleep
    | Bug of string
and TransactionParameters = { FromId: AiCellId; ToId: AiCellId; AmountToTransfer: int }

module AntiCorruptionLayer =
    let convertToAiCells convertToAiCellId (cellId, (cellState: CellStateOwn), neighbours : Cell seq) : AiCell =
        let createNeighbourView neighbour : NeighbourCell =
            match neighbour.State with
            | CellState.Own p when p.AiId = cellState.AiId -> { Id = neighbour.Id |> convertToAiCellId; Owner = CellOwner.Own; Resources = p.Resources }
            | CellState.Own p -> { Id = neighbour.Id |> convertToAiCellId; Owner = CellOwner.Other; Resources = p.Resources }
            | CellState.Free r -> { Id = neighbour.Id |> convertToAiCellId; Owner = CellOwner.None; Resources = r }
    
        {
            Id = cellId |> convertToAiCellId
            Resources = cellState.Resources
            Neighbours = neighbours |> Seq.map createNeighbourView |> Seq.toArray
        }

    let convertToAiPlayed convertToCellId (transactionParameters: TransactionParameters option) : AiActions =
        match transactionParameters with
        | Some t -> AiActions.Transaction { FromId = t.FromId |> convertToCellId; ToId = t.ToId |> convertToCellId; AmountToTransfer = t.AmountToTransfer }
        | Option.None -> AiActions.Sleep

    let wrap convertToAiCellId convertToCellId aiTurn (aiCellsWithNeighbours: AiPlayParameters) : AiActions =
        aiCellsWithNeighbours
        |> Seq.map (convertToAiCells convertToAiCellId)
        |> Seq.toArray
        |> (fun c -> if c.Length = 0 then Option.None else aiTurn c)
        |> convertToAiPlayed convertToCellId

