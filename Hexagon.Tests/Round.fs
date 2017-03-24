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
    let otherAiId = 3

    let cell1OfAi = { Id = { LineNum = 1; ColumnNum = 1 }; State = Own { AiId = aiId; Resources = 5 }}
    let cell2OfAi = { Id = { LineNum = 1; ColumnNum = 2 }; State = Own { AiId = aiId; Resources = 7 }}
    let freeCell = { Id = { LineNum = 1; ColumnNum = 3 }; State = Free 0}
    let cellOfOtherAi = { Id = { LineNum = 1; ColumnNum = 4 }; State = Own { AiId = otherAiId; Resources = 5 }}
    let free2Cell = { Id = { LineNum = 1; ColumnNum = 5 }; State = Free 5}
    
    let getCell = function
        | id when id = cell1OfAi.Id -> cell1OfAi
        | id when id = cell2OfAi.Id -> cell2OfAi
        | id when id = freeCell.Id -> freeCell
        | id when id = cellOfOtherAi.Id -> cellOfOtherAi
        | id when id = free2Cell.Id -> free2Cell
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
            Transaction { FromId = cell1OfAi.Id; ToId = cell2OfAi.Id; AmountToTransfert = 1 } 
            |> expect (Board (
                        ResourcesTransfered { FromId = cell1OfAi.Id; ToId = cell2OfAi.Id; AmountToTransfert = 1 }, [
                            ResourcesChanged { CellId = cell1OfAi.Id; Resources = 4 }
                            ResourcesChanged { CellId = cell2OfAi.Id; Resources = 8 }
                        ], []))

        [<Fact>] 
        member x.``return FightDrawed and attacker lost all resources when fight with same resources amount on own cell`` ()= 
            Transaction { FromId = cell2OfAi.Id; ToId = cellOfOtherAi.Id; AmountToTransfert = 5 } 
            |> expect (Board (
                        FightDrawed { FromId = cell2OfAi.Id; ToId = cellOfOtherAi.Id }, [
                            ResourcesChanged { CellId = cell2OfAi.Id; Resources = 0 }
                            ResourcesChanged { CellId = cellOfOtherAi.Id; Resources = 6 }
                        ], [
                            TerritoryChanged { AiId = aiId; ResourcesIncrement = -7; CellsIncrement = 0 }
                            TerritoryChanged { AiId = otherAiId; ResourcesIncrement = 1; CellsIncrement = 0 }
                        ]))

        [<Fact>] 
        member x.``return FightDrawed and attacker lost all resources when fight with same resources amount on free cell`` ()= 
            Transaction { FromId = cell2OfAi.Id; ToId = free2Cell.Id; AmountToTransfert = 5 } 
            |> expect (Board (
                        FightDrawed { FromId = cell2OfAi.Id; ToId = free2Cell.Id }, [
                            ResourcesChanged { CellId = cell2OfAi.Id; Resources = 0 }
                            ResourcesChanged { CellId = free2Cell.Id; Resources = 6 }
                        ], [
                            TerritoryChanged { AiId = aiId; ResourcesIncrement = -7; CellsIncrement = 0 }
                        ]))

        [<Fact>] 
        member x.``return FightWon and own cell when attacker with more resources on own cell`` ()= 
            Transaction { FromId = cell2OfAi.Id; ToId = cellOfOtherAi.Id; AmountToTransfert = 6 } 
            |> expect (Board (
                        FightWon { FromId = cell2OfAi.Id; ToId = cellOfOtherAi.Id; AmountToTransfert = 6; AiId = aiId }, [
                            ResourcesChanged { CellId = cell2OfAi.Id; Resources = 1 }
                            Owned { CellId = cellOfOtherAi.Id; Resources = 1; AiId = aiId }
                        ], [
                            TerritoryChanged { AiId = aiId; ResourcesIncrement = -5; CellsIncrement = 1 }
                            TerritoryChanged { AiId = otherAiId; ResourcesIncrement = -5; CellsIncrement = -1 }
                        ]))

        [<Fact>] 
        member x.``return FightWon and own cell when attacker with more resources on free cell`` ()= 
            Transaction { FromId = cell2OfAi.Id; ToId = free2Cell.Id; AmountToTransfert = 6 } 
            |> expect (Board (
                        FightWon { FromId = cell2OfAi.Id; ToId = free2Cell.Id; AmountToTransfert = 6; AiId = aiId }, [
                            ResourcesChanged { CellId = cell2OfAi.Id; Resources = 1 }
                            Owned { CellId = free2Cell.Id; Resources = 1; AiId = aiId }
                        ], [
                            TerritoryChanged { AiId = aiId; ResourcesIncrement = -5; CellsIncrement = 1 }
                        ]))

        [<Fact>] 
        member x.``return FightLost when attacker with less resources on own cell`` ()= 
            Transaction { FromId = cell2OfAi.Id; ToId = cellOfOtherAi.Id; AmountToTransfert = 4 } 
            |> expect (Board (
                        FightLost { FromId = cell2OfAi.Id; ToId = cellOfOtherAi.Id; AmountToTransfert = 4 }, [
                            ResourcesChanged { CellId = cell2OfAi.Id; Resources = 3 }
                            ResourcesChanged { CellId = cellOfOtherAi.Id; Resources = 1 }
                        ], [
                            TerritoryChanged { AiId = aiId; ResourcesIncrement = -4; CellsIncrement = 0 }
                            TerritoryChanged { AiId = otherAiId; ResourcesIncrement = -4; CellsIncrement = 0 }
                        ]))

        [<Fact>] 
        member x.``return FightLost when attacker with less resources on free cell`` ()= 
            Transaction { FromId = cell2OfAi.Id; ToId = free2Cell.Id; AmountToTransfert = 4 } 
            |> expect (Board (
                        FightLost { FromId = cell2OfAi.Id; ToId = free2Cell.Id; AmountToTransfert = 4 }, [
                            ResourcesChanged { CellId = cell2OfAi.Id; Resources = 3 }
                            ResourcesChanged { CellId = free2Cell.Id; Resources = 1 }
                        ], [
                            TerritoryChanged { AiId = aiId; ResourcesIncrement = -4; CellsIncrement = 0 }
                        ]))
                        
    type ``generateResourcesIncreased should`` ()=
        [<Fact>] 
        member x.``increment resources of all cells`` ()= 
            let expected = 
                Board (
                    ResourcesIncreased 1, 
                    [ ResourcesChanged { CellId = cell2OfAi.Id; Resources = 8 }
                      ResourcesChanged { CellId = cellOfOtherAi.Id; Resources = 6 } ],
                    [ TerritoryChanged { AiId = aiId; ResourcesIncrement = 1; CellsIncrement = 0 }
                      TerritoryChanged { AiId = otherAiId; ResourcesIncrement = 1; CellsIncrement = 0 }])
            test <@ [ (cell2OfAi.Id, { CellStateOwn.AiId = aiId; Resources = 7 }); (cellOfOtherAi.Id, { AiId = otherAiId; Resources = 5 })] |> generateResourcesIncreased = expected @>

        [<Fact>] 
        member x.``not increment resources of cells with 100 resources`` ()= 
            let expected = Board (ResourcesIncreased 1, [], [])
            test <@ [ (cell2OfAi.Id, { CellStateOwn.AiId = aiId; Resources = 100 }); (cellOfOtherAi.Id, { AiId = otherAiId; Resources = 200 })]  |> generateResourcesIncreased = expected @>

module AiActions =
    open Hexagon.Round.AiActions
        
    type ``playAi should`` ()=
        [<Fact>] 
        member x.``play ai with neighbours cells and return GameEvents`` ()= 
            let aiId = 1
            let cellsWithNeighbours = [({ LineNum = 1; ColumnNum = 1 }, { CellStateOwn.AiId = aiId; Resources = 5 }, [{ Id = { LineNum = 1; ColumnNum = 2 }; State = Own { AiId = aiId; Resources = 5 }}])]
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
            [({ LineNum = 1; ColumnNum = 2 }, { CellStateOwn.AiId = 1; Resources = 5 })]

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
