module Hexagon.Tests.CellsStore

open System
open Xunit
open FsUnit.Xunit
open Swensen.Unquote

open Hexagon.Domain
open Hexagon.CellsStore
    
let cellId1 = { LineNum = 1; ColumnNum = 1 }
let cell1 = { Id = cellId1; State = Free 1 }
let cellId2 = { LineNum = 1; ColumnNum = 2 }
let cell2 = { Id = cellId2; State = Free 1 }
let board = [
        cell1
        cell2
        { Id = { LineNum = 2; ColumnNum = 1 }; State = Free 1 }
        { Id = { LineNum = 2; ColumnNum = 2 }; State = Free 1 }
    ]

let isNeighbours _ _ = true

type ``Store should`` ()=
    [<Fact>] 
    member x.``return cell when getCell with id`` ()= 
        let store = CellsStore(board, isNeighbours)

        test <@ store.getCell cellId1 = Some cell1 @>

    [<Fact>] 
    member x.``return none when getCell with unknown id`` ()= 
        let store = CellsStore(board, isNeighbours)
        let unknownId = { LineNum = 9; ColumnNum = 1 }

        test <@ store.getCell unknownId = None @>

    [<Fact>] 
    member x.``return all cells of ai when getCellsOf`` ()= 
        let aiId = 1
        let board = [ 
                { cell1 with State = Own { AiId = aiId; Resources = 5 } }
                { Id = { LineNum = 2; ColumnNum = 1 }; State = Own { AiId = 2; Resources = 5 } }
                { cell2 with State = Own { AiId = aiId; Resources = 5 } }
                { Id = { LineNum = 2; ColumnNum = 2 }; State = Free 5 }
            ]
        let store = CellsStore(board, isNeighbours)

        test <@ store.getCellsOf aiId = [ (cellId1, { AiId = aiId; Resources = 5 }); (cellId2, { AiId = aiId; Resources = 5 }) ] @>

    [<Fact>] 
    member x.``return neighbours of cell when getNeighbours`` ()= 
        let cellId3 = { LineNum = 3; ColumnNum = 1 }
        let board = [ 
                cell1
                { Id = { LineNum = 2; ColumnNum = 1 }; State = Own { AiId = 2; Resources = 5 } }
                cell2
                { Id = cellId3; State = Free 5 }
            ]
        let isNeighbours _ = function
            | id when id = cellId2 -> true
            | id when id = cellId3 -> true
            | _ -> false
        let store = CellsStore(board, isNeighbours)

        test <@ store.getNeighbours cellId1 = [ cell2; board.[3] ] @>

    [<Fact>] 
    member x.``indicate if is neighbours when isNeighboursOf`` ()= 
        let cellId3 = { LineNum = 3; ColumnNum = 1 }
        let board = [ 
                cell1
                cell2
                { Id = cellId3; State = Free 5 }
            ]
        let isNeighbours id1 id2 =
            match id1, id2 with
            | id, _ when id = cellId1 -> true
            | _, id when id = cellId2 -> true
            | _ -> false
        let store = CellsStore(board, isNeighbours)

        test <@ store.isNeighboursOf cellId1 cellId2 = isNeighbours cellId1 cellId2 @>
        test <@ store.isNeighboursOf cellId1 cellId3 = isNeighbours cellId1 cellId3 @>

    [<Fact>] 
    member x.``return all cells of ai with this neighbours when getCellsWithNeighboursOf`` ()= 
        let aiId = 1
        let board = [ 
                { cell1 with State = Own { AiId = aiId; Resources = 5 } }
                { Id = { LineNum = 2; ColumnNum = 1 }; State = Own { AiId = 2; Resources = 5 } }
                { cell2 with State = Own { AiId = aiId; Resources = 5 } }
                { Id = { LineNum = 2; ColumnNum = 2 }; State = Free 5 }
                { Id = { LineNum = 3; ColumnNum = 2 }; State = Free 5 }
            ]
        let isNeighbours _ = function
            | id when id.LineNum < 3 -> true
            | _ -> false
        let store = CellsStore(board, isNeighbours)

        let expected = [
                (cellId1, { CellStateOwn.AiId = aiId; Resources = 5 }, store.getNeighbours cellId1)
                (cellId2, { CellStateOwn.AiId = aiId; Resources = 5 }, store.getNeighbours cellId2) 
            ]
        test <@ store.getCellsWithNeighboursOf aiId = expected @>

    [<Fact>] 
    member x.``return all own cells with id and resources when getAllOwnCells`` ()= 
        let board = [ 
                { cell1 with State = Own { AiId = 1; Resources = 5 } }
                { cell2 with State = Own { AiId = 2; Resources = 6 } }
                { Id = { LineNum = 2; ColumnNum = 2 }; State = Free 5 }
            ]
        let store = CellsStore(board, isNeighbours)
        
        test <@ store.getAllOwnCells() = [ (cellId1, {AiId = 1; Resources = 5 }); (cellId2, { AiId = 2; Resources = 6 }) ] @>

    [<Fact>] 
    member x.``update own state when Owned`` ()= 
        let board = [ 
                { cell1 with State = Own { AiId = 1; Resources = 5 } }
                { Id = { LineNum = 2; ColumnNum = 2 }; State = Free 5 }
            ]
        let store = CellsStore(board, isNeighbours)

        store.apply (Owned { CellId = cellId1; AiId = 2; Resources = 8 })
        
        test <@ store.getCell cellId1 = Some { cell1 with State = Own { AiId = 2; Resources = 8 } } @>

    [<Fact>] 
    member x.``update resource of own cell when ResourcesChanged`` ()= 
        let board = [ 
                { cell1 with State = Own { AiId = 1; Resources = 5 } }
                { Id = { LineNum = 2; ColumnNum = 2 }; State = Free 5 }
            ]
        let store = CellsStore(board, isNeighbours)

        store.apply (ResourcesChanged { CellId = cellId1; Resources = 8 })
        
        test <@ store.getCell cellId1 = Some { cell1 with State = Own { AiId = 1; Resources = 8 } } @>

    [<Fact>] 
    member x.``update resource of free cell when ResourcesChanged`` ()= 
        let board = [ 
                { cell1 with State = Free 6 }
                { Id = { LineNum = 2; ColumnNum = 2 }; State = Free 5 }
            ]
        let store = CellsStore(board, isNeighbours)

        store.apply (ResourcesChanged { CellId = cellId1; Resources = 8 })
        
        test <@ store.getCell cellId1 = Some { cell1 with State = Free 8 } @>
