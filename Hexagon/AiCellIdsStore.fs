module Hexagon.AiCellIdsStore

open Domain

let generateGuid _ = System.Guid.NewGuid().ToString()

type Store (cells: Board)=
    let cellIds = cells |> Seq.map (fun c -> c.Id, generateGuid()) |> dict
    let aiCellIds = cellIds |> Seq.map (fun v -> v.Value, v.Key) |> dict

    member x.convertToAiCellId id =
        cellIds.[id]

    member x.convertToCellId id =
        aiCellIds.[id]

