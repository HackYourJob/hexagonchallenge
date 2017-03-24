module Hexagon.Ais

open Domain

type AiCellId = string
type AiCell = {
        Id: AiCellId
        Resources: int
        Neighbours: NeighbourCell list
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
and TransactionParameters = { FromId: AiCellId; ToId: AiCellId; AmountToTransfert: int }

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
            Neighbours = neighbours |> Seq.map createNeighbourView |> Seq.toList
        }

    let convertToAiPlayed convertToCellId (transaction: Transaction) : AiActions =
        match transaction with
        | Transaction.Sleep -> AiActions.Sleep
        | Transaction.Bug r -> AiActions.Bug r
        | Transaction.Move t -> AiActions.Transaction { FromId = t.FromId |> convertToCellId; ToId = t.ToId |> convertToCellId; AmountToTransfert = t.AmountToTransfert }

    let wrap convertToAiCellId convertToCellId aiTurn (aiCellsWithNeighbours: AiPlayParameters) : AiActions =
        aiCellsWithNeighbours
        |> Seq.map (convertToAiCells convertToAiCellId)
        |> Seq.toList
        |> (fun c -> if c.IsEmpty then Transaction.Sleep else aiTurn c)
        |> convertToAiPlayed convertToCellId

