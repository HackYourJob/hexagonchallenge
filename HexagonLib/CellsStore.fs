module Hexagon.CellsStore

open Domain

type CellsStore (cells: Board, isNeighbours: CellId -> CellId -> bool)=
    let cellsById = System.Collections.Generic.Dictionary<_,_>(cells |> Seq.map (fun c -> c.Id, c) |> dict)
    let neighboursById = cellsById.Keys |> Seq.map (fun id -> id, cellsById.Keys |> Seq.filter (fun nId -> isNeighbours id nId) |> Seq.toList) |> dict

    let tryGetValue (dict: System.Collections.Generic.IDictionary<_, _>) key =
        match dict.TryGetValue(key) with
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
                cellsById.[cellId] <- { cellsById.[cellId] with State = Own { AiId = aiId; Resources = resources } }
        | ResourcesChanged { CellId = cellId; Resources = resources } ->
                let cell = cellsById.[cellId]
                cellsById.[cellId] <- 
                        match cell.State with 
                        | Free p -> { cell with State = Free resources}
                        | Own p -> { cell with State = Own { p with Resources = resources }}

         

