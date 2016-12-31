module Hexagon.Tests.Round

open System
open Xunit
open FsUnit.Xunit

open Hexagon.Domain

module TransactionValidation =
    open Hexagon.Round.TransactionValidation
    
    let aiId = 1

    let cellOfAi = { Id = { LineNum = 1; ColumnNum = 1 }; State = Own { AiId = aiId; Resources = 5 }}
    let freeCell = { Id = { LineNum = 1; ColumnNum = 2 }; State = Free 0}
    let cellOfOtherAi = { Id = { LineNum = 1; ColumnNum = 3 }; State = Own { AiId = aiId + 2; Resources = 5 }}
    let unknownCellId = { LineNum = 1; ColumnNum = 4 }

    let neighboursCell = { Id = { LineNum = 2; ColumnNum = 2 }; State = Free 0}
    let notNeighboursCell = { Id = { LineNum = 2; ColumnNum = 3 }; State = Free 0}

    let getCell = function
        | id when id = cellOfAi.Id -> Some cellOfAi
        | id when id = freeCell.Id -> Some freeCell
        | id when id = cellOfOtherAi.Id -> Some cellOfOtherAi
        | id when id = neighboursCell.Id -> Some neighboursCell
        | id when id = notNeighboursCell.Id -> Some notNeighboursCell
        | _ -> None
    let isNeighboursOf id1 id2 = id1 = cellOfAi.Id && id2 = neighboursCell.Id

    let validAction = validAiAction getCell isNeighboursOf aiId

    type ``validAiAction should`` ()=
        [<Fact>] 
        member x.``return same value if sleep`` ()= 
            Sleep
            |> validAction
            |> should equal Sleep

        [<Fact>] 
        member x.``return same value if bug`` ()= 
            Bug "Error"
            |> validAction
            |> should equal (Bug "Error")

        [<Fact>] 
        member x.``return bug if transaction not start of ai's cell`` ()= 
            Transaction { FromId = cellOfOtherAi.Id; ToId = neighboursCell.Id; AmountToTransfert = 1 }
            |> validAction
            |> should equal (Bug "NotOwnCell")

        [<Fact>] 
        member x.``return bug if transaction start of free cell`` ()= 
            Transaction { FromId = freeCell.Id; ToId = neighboursCell.Id; AmountToTransfert = 1 }
            |> validAction
            |> should equal (Bug "NotOwnCell")

        [<Fact>] 
        member x.``return bug if transaction move too resources`` ()= 
            Transaction { FromId = cellOfAi.Id; ToId = neighboursCell.Id; AmountToTransfert = 6 }
            |> validAction
            |> should equal (Bug "NotEnoughResources")

        [<Fact>] 
        member x.``return bug if transaction not end of neighbours`` ()= 
            Transaction { FromId = cellOfAi.Id; ToId = notNeighboursCell.Id; AmountToTransfert = 1 }
            |> validAction
            |> should equal (Bug "InvalidMove")

        [<Fact>] 
        member x.``return bug if transaction start of unknown cell`` ()= 
            Transaction { FromId = unknownCellId; ToId = neighboursCell.Id; AmountToTransfert = 1 }
            |> validAction
            |> should equal (Bug "NotOwnCell")

        [<Fact>] 
        member x.``returntransaction if transaction is valid`` ()= 
            let validTransaction = Transaction { FromId = cellOfAi.Id; ToId = neighboursCell.Id; AmountToTransfert = 5 }
            validTransaction
            |> validAction
            |> should equal validTransaction