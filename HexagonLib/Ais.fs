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

    let convertToAiPlayed (tryConvertToCellId: AiCellId -> CellId option) (transactionParameters: TransactionParameters option) : AiActions =
        match transactionParameters with
        | Some t -> 
            match t.FromId |> tryConvertToCellId, t.ToId |> tryConvertToCellId with
            | Option.None, _ -> AiActions.Bug "Invalid FromId"
            | _, Option.None -> AiActions.Bug "Invalid ToId"
            | Some fromId, Some toId -> 
                AiActions.Transaction { FromId = fromId; ToId = toId; AmountToTransfer = t.AmountToTransfer }
        | Option.None -> AiActions.Sleep

    let wrap convertToAiCellId tryConvertToCellId aiTurn (aiCellsWithNeighbours: AiPlayParameters) : AiActions =
        try
            aiCellsWithNeighbours
            |> Seq.map (convertToAiCells convertToAiCellId)
            |> Seq.toArray
            |> (fun c -> if c.Length = 0 then Option.None else aiTurn c)
            |> convertToAiPlayed tryConvertToCellId
        with | ex -> AiActions.Bug (ex.ToString())

