module Hexagon.Tests.ScoresStore

open System
open Xunit
open FsUnit.Xunit
open Swensen.Unquote

open Hexagon.Domain
open Hexagon.ScoresStore
    
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
    member x.``Increment bug of Ai When apply Bugged`` ()= 
        let store = Store()

        store.apply (Bugged 1)
        test <@ store.getScores() = [(1, { CellsNb = 0; Resources = 0; BugsNb = 1 })] @>

        store.apply (Bugged 1)
        test <@ store.getScores() = [(1, { CellsNb = 0; Resources = 0; BugsNb = 2 })] @>

        store.apply (Bugged 2)
        test <@ store.getScores() = [(1, { CellsNb = 0; Resources = 0; BugsNb = 2 }); (2, { CellsNb = 0; Resources = 0; BugsNb = 1 })] @>

    [<Fact>] 
    member x.``Increment resources and cells of Ai When apply TerritoryChanged`` ()= 
        let store = Store()

        store.apply (TerritoryChanged { AiId = 1; ResourcesIncrement = 11; CellsIncrement = 102 })
        test <@ store.getScores() = [(1, { CellsNb = 102; Resources = 11; BugsNb = 0 })] @>
        
        store.apply (TerritoryChanged { AiId = 1; ResourcesIncrement = 13; CellsIncrement = 104 })
        test <@ store.getScores() = [(1, { CellsNb = 206; Resources = 24; BugsNb = 0 })] @>
        
        store.apply (TerritoryChanged { AiId = 2; ResourcesIncrement = 1; CellsIncrement = 2 })
        test <@ store.getScores() = [(1, { CellsNb = 206; Resources = 24; BugsNb = 0 }); (2, { CellsNb = 2; Resources = 1; BugsNb = 0 })] @>
        
    [<Fact>] 
    member x.``return aiId only if one ai with cell when tryToGetWinner`` ()= 
        let store = Store()
        store.apply (TerritoryChanged { AiId = 1; ResourcesIncrement = 1; CellsIncrement = 1 })
        store.apply (TerritoryChanged { AiId = 2; ResourcesIncrement = 1; CellsIncrement = 1 })

        test <@ store.tryToGetWinner() = None @>
        
        store.apply (TerritoryChanged { AiId = 2; ResourcesIncrement = -1; CellsIncrement = -1 })

        test <@ store.tryToGetWinner() = Some 1 @>

