module Hexagon.CellsStore

open Domain

type CellsStore (cells: Board, isNeighbours: CellId -> CellId -> bool)=
    let serializeCellId cellId = sprintf "%dx%d" cellId.LineNum cellId.ColumnNum

    let cellsById = System.Collections.Generic.Dictionary<_,_>(cells |> Seq.map (fun c -> c.Id |> serializeCellId, c) |> dict)
    let neighboursById = 
        let filterNeighbours cellId cells =
            cells 
            |> Seq.filter (fun neighbours -> isNeighbours cellId neighbours.Id) 
            |> Seq.map (fun cell -> serializeCellId cell.Id) 
            |> Seq.toList

        cells 
        |> Seq.map (fun cell -> cell.Id |> serializeCellId, cells |> filterNeighbours cell.Id) 
        |> dict

    let tryGetValue (dict: System.Collections.Generic.IDictionary<_, _>) key =
        match dict.TryGetValue(key |> serializeCellId) with
        | true, value -> Some value
        | false, _ -> None

    member x.getCellsOf aiId =
        cellsById.Values 
        |> Seq.map (fun c -> match c.State with | Own p -> Some (c.Id, p) | Free _ -> Option.None)
        |> Seq.choose id
        |> Seq.filter (fun (id, c) -> c.AiId = aiId)
        |> Seq.toList

    member x.getCellsWithNeighboursOf aiId =
        x.getCellsOf aiId 
        |> Seq.map (fun (id, c) -> id, c, id |> x.getNeighbours)
        |> Seq.toList

    member x.getNeighbours cellId =
        match tryGetValue neighboursById cellId with
        | None -> []
        | Some ids -> ids |> List.map (fun id -> cellsById.[id])

    member x.getCell (cellId: CellId) : Cell option =
        tryGetValue cellsById cellId

    member x.isNeighboursOf referenceId otherId =
         isNeighbours referenceId otherId

    member x.getAllOwnCells () =
        cellsById.Values 
        |> Seq.map (fun c -> match c.State with | Own param -> Some (c.Id, param) | Free _ -> None)
        |> Seq.choose id
        |> Seq.toList

    member x.apply (evt: CellChanged) =
        match evt with
        | Owned { CellId = cellId; AiId = aiId; Resources = resources } -> 
                let serializedId = serializeCellId cellId
                cellsById.[serializedId] <- { cellsById.[serializedId] with State = Own { AiId = aiId; Resources = resources } }
        | ResourcesChanged { CellId = cellId; Resources = resources } ->
                let serializedId = serializeCellId cellId
                let cell = cellsById.[serializedId]
                cellsById.[serializedId] <- 
                        match cell.State with 
                        | Free p -> { cell with State = Free resources}
                        | Own p -> { cell with State = Own { p with Resources = resources }}
