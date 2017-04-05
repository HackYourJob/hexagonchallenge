module Hexagon.Tests.AiCellIdsStore

open System
open Xunit
open FsUnit.Xunit
open Swensen.Unquote

open Hexagon.Domain
open Hexagon.AiCellIdsStore
    
let cellId1 = { LineNum = 1; ColumnNum = 1 }
let cellId2 = { LineNum = 1; ColumnNum = 2 }
let board = [
        { Id = cellId1; State = Free 1; IsStartingPosition = false }
        { Id = cellId2; State = Free 1; IsStartingPosition = false }
        { Id = { LineNum = 2; ColumnNum = 1 }; State = Free 1; IsStartingPosition = false }
        { Id = { LineNum = 2; ColumnNum = 2 }; State = Free 1; IsStartingPosition = false }
    ]

type ``Store should`` ()=
    [<Fact>] 
    member x.``always return same AiCellId for same CellId`` ()= 
        let store = Store(board)

        test <@ store.convertToAiCellId cellId1 = store.convertToAiCellId cellId1 @>

    [<Fact>] 
    member x.``generate unique id for each cells`` ()= 
        let store = Store(board)

        test <@ store.convertToAiCellId cellId1 <> store.convertToAiCellId cellId2 @>

    [<Fact>] 
    member x.``always get CellId of AiCellId`` ()= 
        let store = Store(board)
        let aiCellId = store.convertToAiCellId cellId1

        test <@ store.convertToCellId aiCellId = cellId1 @>

    [<Fact>] 
    member x.``generate random ids`` ()= 
        let store1 = Store(board)
        let store2 = Store(board)

        test <@ store1.convertToAiCellId cellId1 <> store2.convertToAiCellId cellId1 @>
