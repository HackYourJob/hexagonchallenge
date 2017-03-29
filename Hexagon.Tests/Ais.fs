module Hexagon.Tests.Ais

open System
open Xunit
open FsUnit.Xunit
open Swensen.Unquote

open Hexagon.Domain

module AntiCorruptionLayer =
    open Hexagon.Ais
    open Hexagon.Ais.AntiCorruptionLayer
    
    let cellId = { LineNum = 1; ColumnNum = 1 }
    let cellState = { CellStateOwn.AiId = 1; Resources = 5 }
    let cellId2 = { LineNum = 1; ColumnNum = 2 }

    type ``convertToAiCells should`` ()=
        [<Fact>] 
        member x.``convert to ai object`` ()= 
            let neighbours = [{ Id = cellId2; State = CellState.Free 8}]
            let convertToAiCellId = function
                | c when c = cellId -> "a"
                | c when c = cellId2 -> "b"
                | _ -> failwith "invalid cell"

            let result = convertToAiCells convertToAiCellId (cellId, cellState, neighbours)

            test <@ result = { Id = "a"; Resources = 5; Neighbours = [|{ Id = "b"; Owner = CellOwner.None; Resources = 8}|] } @>

        [<Fact>] 
        member x.``Neighbour own if has same id`` ()= 
            let neighbours = [{ Id = cellId2; State = CellState.Own { AiId = 1; Resources = 8}}]
            let convertToAiCellId _ = "b"

            let { Id = _; Resources = _; Neighbours = neighbours } = convertToAiCells convertToAiCellId (cellId, cellState, neighbours)
            
            test <@ neighbours = [|{ Id = "b"; Owner = CellOwner.Own; Resources = 8}|] @>

        [<Fact>] 
        member x.``Neighbour Other if has not same id`` ()= 
            let neighbours = [{ Id = cellId2; State = CellState.Own { AiId = 2; Resources = 8}}]
            let convertToAiCellId _ = "b"

            let { Id = _; Resources = _; Neighbours = neighbours } = convertToAiCells convertToAiCellId (cellId, cellState, neighbours)

            test <@ neighbours = [|{ Id = "b"; Owner = CellOwner.Other; Resources = 8}|] @>

    type ``convertToAiPlayed should`` ()=
        [<Fact>] 
        member x.``return Sleep if sleep`` ()= 
            let convertToCellId _ = failwith "Not used"

            let result = Option.None |> convertToAiPlayed convertToCellId 

            test <@ result = AiActions.Sleep @>
                        
        [<Fact>] 
        member x.``return Transaction if Move`` ()= 
            let convertToCellId = function
                | "a" -> cellId
                | "b" -> cellId2
                | _ -> failwith "invalid id"

            let result = Some { FromId = "a"; ToId = "b"; AmountToTransfer = 5 } |> convertToAiPlayed convertToCellId 

            test <@ result = AiActions.Transaction { FromId = cellId; ToId = cellId2; AmountToTransfer = 5 } @>

    type ``wrap should`` ()=
        [<Fact>] 
        member x.``create layer`` ()= 
            let convertToCellId = function
                | "a" -> cellId
                | "b" -> cellId2
                | _ -> failwith "invalid id"
            let convertToAiCellId = function
                | c when c = cellId -> "a"
                | c when c = cellId2 -> "b"
                | c -> sprintf "invalid cell %A" c |> failwith

            let aiTurn = function 
                | c when c = [|{ Id = "a"; Resources = 5; Neighbours = [|{ Id = "b"; Owner = CellOwner.None; Resources = 8}|] }|]
                    -> Some { FromId = "a"; ToId = "b"; AmountToTransfer = 5 }
                | c -> sprintf "invalid cells %A" c |> failwith
            let cells = [(cellId, cellState, [{ Id = cellId2; State = CellState.Free 8}])]

            let result = wrap convertToAiCellId convertToCellId aiTurn cells

            test <@ result = AiActions.Transaction { FromId = cellId; ToId = cellId2; AmountToTransfer = 5 } @>

        [<Fact>] 
        member x.``not play ai if not cells (game over)`` ()= 
            let convertToCellId _ = cellId
            let convertToAiCellId _ = "a"
            let aiTurn = function 
                | _ -> failwith "invalid cells"

            let result = wrap convertToAiCellId convertToCellId aiTurn []

            test <@ result = AiActions.Sleep @>
