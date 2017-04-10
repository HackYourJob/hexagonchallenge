module Hexagon.BasicAi

open Ais

let play (cells: AiCell []) : TransactionParameters option =
    cells 
    |> Seq.collect (fun c -> c.Neighbours |> Seq.map (fun n -> c, n))
    |> Seq.filter (fun (_, n) -> n.Owner <> Own)
    |> Seq.sortByDescending (fun (c, n) -> c.Resources - n.Resources)
    |> Seq.head
    |> (fun (c, n) -> Some { FromId = c.Id; ToId = n.Id; AmountToTransfer = c.Resources })
