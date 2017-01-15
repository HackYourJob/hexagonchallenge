module Hexagon.Tests.Round

open System
open Xunit
open FsUnit.Xunit
open Swensen.Unquote

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

module BoardHandler =
    open Hexagon.Round.BoardHandler
    
    let aiId = 1

    let cellOfAi1 = { Id = { LineNum = 1; ColumnNum = 1 }; State = Own { AiId = aiId; Resources = 5 }}
    let cellOfAi2 = { Id = { LineNum = 1; ColumnNum = 2 }; State = Own { AiId = aiId; Resources = 7 }}
    let freeCell = { Id = { LineNum = 1; ColumnNum = 3 }; State = Free 0}
    let cellOfOtherAi = { Id = { LineNum = 1; ColumnNum = 4 }; State = Own { AiId = aiId + 2; Resources = 5 }}
    
    let getCell = function
        | id when id = cellOfAi1.Id -> cellOfAi1
        | id when id = cellOfAi2.Id -> cellOfAi2
        | id when id = freeCell.Id -> freeCell
        | id when id = cellOfOtherAi.Id -> cellOfOtherAi
        | _ -> failwith "invalid cell"

    let generate action = generateEvents getCell action |> Seq.toList

    let expect expected action =
        test <@ action |> generate = [expected] @>
    
    type ``generateEvents should`` ()=
        [<Fact>] 
        member x.``return empty if sleep`` ()= 
            test <@ Sleep |> generate = [] @>

        [<Fact>] 
        member x.``return empty if bug`` ()= 
            test <@ Bug "Error" |> generate = [] @>

        [<Fact>] 
        member x.``return ResourcesTransfered when move between two cells of same ai`` ()= 
            Transaction { FromId = cellOfAi1.Id; ToId = cellOfAi2.Id; AmountToTransfert = 1 } 
            |> expect (Board (
                        ResourcesTransfered { FromId = cellOfAi1.Id; ToId = cellOfAi2.Id; AmountToTransfert = 1 }, [
                            ResourcesChanged { CellId = cellOfAi1.Id; Resources = 4 }
                            ResourcesChanged { CellId = cellOfAi2.Id; Resources = 8 }
                        ]))

        [<Fact>] 
        member x.``return FightDrawed and attacker lost all resources when fight with same resources amount`` ()= 
            Transaction { FromId = cellOfAi2.Id; ToId = cellOfOtherAi.Id; AmountToTransfert = 5 } 
            |> expect (Board (
                        FightDrawed { FromId = cellOfAi2.Id; ToId = cellOfOtherAi.Id }, [
                            ResourcesChanged { CellId = cellOfAi2.Id; Resources = 0 }
                            ResourcesChanged { CellId = cellOfOtherAi.Id; Resources = 6 }
                        ]))

        [<Fact>] 
        member x.``return FightWon and own cell when attacker with more resources`` ()= 
            Transaction { FromId = cellOfAi2.Id; ToId = cellOfOtherAi.Id; AmountToTransfert = 6 } 
            |> expect (Board (
                        FightWon { FromId = cellOfAi2.Id; ToId = cellOfOtherAi.Id; AmountToTransfert = 6; AiId = aiId }, [
                            ResourcesChanged { CellId = cellOfAi2.Id; Resources = 1 }
                            Owned { CellId = cellOfOtherAi.Id; Resources = 1; AiId = aiId }
                        ]))

        [<Fact>] 
        member x.``return FightLost when attacker with less resources`` ()= 
            Transaction { FromId = cellOfAi2.Id; ToId = cellOfOtherAi.Id; AmountToTransfert = 4 } 
            |> expect (Board (
                        FightLost { FromId = cellOfAi2.Id; ToId = cellOfOtherAi.Id; AmountToTransfert = 4 }, [
                            ResourcesChanged { CellId = cellOfAi2.Id; Resources = 3 }
                            ResourcesChanged { CellId = cellOfOtherAi.Id; Resources = 1 }
                        ]))
                        
    type ``generateResourcesIncreased should`` ()=
        [<Fact>] 
        member x.``increment resources of all cells`` ()= 
            let expected = 
                Board (
                    ResourcesIncreased 1, 
                    [ ResourcesChanged { CellId = cellOfAi2.Id; Resources = 8 }
                      ResourcesChanged { CellId = cellOfOtherAi.Id; Resources = 6 } ])
            test <@ [ (cellOfAi2.Id, 7); (cellOfOtherAi.Id, 5)] |> generateResourcesIncreased = expected @>

        [<Fact>] 
        member x.``not increment resources of cells with 100 resources`` ()= 
            let expected = Board (ResourcesIncreased 1, [])
            test <@ [ (cellOfAi2.Id, 100); (cellOfOtherAi.Id, 200)]  |> generateResourcesIncreased = expected @>

module AiActions =
    open Hexagon.Round.AiActions
        
    type ``playAi should`` ()=
        [<Fact>] 
        member x.``play ai with neighbours cells and return GameEvents`` ()= 
            let aiId = 1
            let cellsWithNeighbours = [({ LineNum = 1; ColumnNum = 1 }, { AiId = aiId; Resources = 5 }, [{ Id = { LineNum = 1; ColumnNum = 2 }; State = Own { AiId = aiId; Resources = 5 }}])]
            let aiAction = Transaction { FromId = { LineNum = 1; ColumnNum = 1 }; ToId = { LineNum = 1; ColumnNum = 2 }; AmountToTransfert = 5 }
            let aiActionAfterValidation = Transaction { FromId = { LineNum = 1; ColumnNum = 1 }; ToId = { LineNum = 1; ColumnNum = 2 }; AmountToTransfert = 2 }

            let getCellsWithNeighboursOf = function
                | id when id = aiId -> cellsWithNeighbours
                | _ -> failwith "Invalid aiId"

            let getCell = function
                | id -> { Id = id; State = Own { AiId = aiId; Resources = 5 }}

            let validAction aiId = function
                | a when a = aiAction -> aiActionAfterValidation
                | _ -> failwith "Invalid action"

            let play = function
                | c when c = cellsWithNeighbours -> aiAction
                | _ -> failwith "Invalid cells"
                

            let result = playAi getCellsWithNeighboursOf getCell validAction (aiId, play) |> Seq.toList
            
            let expected = AiPlayed aiActionAfterValidation :: Hexagon.Round.BoardHandler.generateEvents getCell aiActionAfterValidation
            test <@ result = expected @>

open Hexagon.Round

type ``runRound should`` ()=
    [<Fact>] 
    member x.``play all ais and increment resources`` ()= 
        let getAllOwnCells () = 
            [({ LineNum = 1; ColumnNum = 2 }, 5)]

        let playAi (id, play) =
            seq {
                yield AiActions.Transaction { FromId = { LineNum = 1; ColumnNum = 1 }; ToId = { LineNum = 1; ColumnNum = 2 }; AmountToTransfert = id } |> AiPlayed
            }
        let ais = [(1,fun _ -> Sleep); (2,fun _ -> Sleep)] |> List.toSeq

        let result = runRound getAllOwnCells playAi ais |> Seq.toList
            
        let expected = [
            AiPlayed (AiActions.Transaction { FromId = { LineNum = 1; ColumnNum = 1 }; ToId = { LineNum = 1; ColumnNum = 2 }; AmountToTransfert = 1 }) 
            AiPlayed (AiActions.Transaction { FromId = { LineNum = 1; ColumnNum = 1 }; ToId = { LineNum = 1; ColumnNum = 2 }; AmountToTransfert = 2 })
            BoardHandler.generateResourcesIncreased (getAllOwnCells ())]
        test <@ result = expected @>
