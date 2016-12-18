module Hexagon.Domain

type AiId = int
type AiDescription = { 
        Id: AiId
        Name: string
    }

type CellId = string
type Cell = {
        Id: CellId
        LineNum: int
        ColumnNum: int
        State: CellState
    }
and CellState =
    | Own of CellStateOwn
    | Free of int
and CellStateOwn = { AiId: AiId; Resources: int }

type Board = Cell seq

