module Hexagon.Domain

type AiId = int
type AiDescription = { 
        Id: AiId
        Name: string
    }

type CellId = { LineNum: int; ColumnNum: int }
type Cell = {
        Id: CellId
        State: CellState
    }
and CellState =
    | Own of CellStateOwn
    | Free of int
and CellStateOwn = { AiId: AiId; Resources: int }

type Board = Cell seq

