module Hexagon.Domain

type AiId = int
type AiDescription = { 
        Id: AiId
        Name: string
    }
type AiScore = { CellsNb: int; Resources: int; BugsNb: int }

type CellId = { LineNum: int; ColumnNum: int }
type Cell = {
        Id: CellId
        State: CellState
        IsStartingPosition: bool
    }
and CellState =
    | Own of CellStateOwn
    | Free of int
and CellStateOwn = { AiId: AiId; Resources: int }

type Board = Cell seq

type GameEvents = 
    | Started of Started
    | AiPlayed of AiActions
    | Board of BoardEvents * (CellChanged list) * (ScoreChanged list)
    | Won of AiId
and Started = { BoardSize: BoardSize; Board: Board; Ais: AiDescription seq }
and BoardSize = { Lines: int; Columns: int }
and AiActions =
    | Transaction of TransactionParameters
    | Bug of string
    | Sleep
and TransactionParameters = { FromId: CellId; ToId: CellId; AmountToTransfer: int }
and BoardEvents =
    | AiAdded of AiAdded
    | ResourcesIncreased of int
    | ResourcesTransfered of TransactionParameters
    | FightWon of FightWon
    | FightDrawed of FightDrawed
    | FightLost of TransactionParameters
    | Bugged
and AiAdded = { AiId: AiId; CellId: CellId; Resources: int }
and FightWon = { FromId: CellId; ToId: CellId; AmountToTransfer: int; AiId: AiId }
and FightDrawed = { FromId: CellId; ToId: CellId }
and CellChanged =
    | Owned of CellOwned
    | ResourcesChanged of CellResourcesChanged
and CellOwned = { CellId: CellId; AiId: AiId; Resources: int }
and CellResourcesChanged = { CellId: CellId; Resources: int }
and ScoreChanged = 
    | Bugged of AiId
    | TerritoryChanged of TerritoryChanged
and TerritoryChanged = { AiId: AiId; ResourcesIncrement: int; CellsIncrement: int }

type AiPlayParameters = (CellId * CellStateOwn * Cell list) list

let extractResources cell =
    match cell.State with
    | Own param -> param.Resources
    | Free resources -> resources