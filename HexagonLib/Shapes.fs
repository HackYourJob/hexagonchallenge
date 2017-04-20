module Hexagon.Shapes

type ShapeCell = StartingCell | Cell | None
type BoardShape = ShapeCell seq seq

let none _ = None
let oddSeq i = if i%2 = 0 then None else Cell

module HexagonBoard =
    let private generateLine size num =
        seq {
            yield! [1 .. size-num] |> Seq.map none
            yield! [1 .. (1 + (num - 1) * 2)] |> Seq.map oddSeq
            yield! [1 .. size-num] |> Seq.map none 
        }

    let private generateHeader size =
        [1 .. size - 1] |> Seq.map (generateLine size)

    let private generateFooter = generateHeader >> Seq.rev

    let private generateBody size =
        let fullLineWithOldLine _ = 
            seq {
                yield generateLine size size
                yield generateLine size (size-1)
            }
            
        [1 .. size] 
        |> Seq.collect fullLineWithOldLine 
        |> Seq.take (size * 2 - 1)

    let private isInTheMiddle j size =
        j + 1 = size

    let private isBetweenTopOrBottomAndCenter i size =
        i + 1 = size || i + 1 = 3 * size - 2

    let private isStartingCell i j size =
        let lastCellIndexByLine = 2 * size - 2
        let lastCellIndexByColumn = 4 * size - 3
        let borderDistanceOnLine = size / 2
        let borderDistanceOnColumns = size - 1
        let isOnALineCrossingLeftToRightDiagonals =
            i = borderDistanceOnColumns + 2
                || i = lastCellIndexByColumn - borderDistanceOnColumns - 3
        let isBetweenBorderAndCenterOnVerticalDiagonal =
            j + 1 = size
            && (i = borderDistanceOnColumns
                || i = lastCellIndexByColumn - borderDistanceOnColumns - 1)
        (j = borderDistanceOnLine && isOnALineCrossingLeftToRightDiagonals)
            || isBetweenBorderAndCenterOnVerticalDiagonal
            || (j = lastCellIndexByLine - borderDistanceOnLine && isOnALineCrossingLeftToRightDiagonals)

    let private transform shape i j board size =
        match shape with
        | Cell -> 
            if shape = Cell && isStartingCell i j size  then
                StartingCell
            else
                shape
        | _ -> shape

    let generate (size: int) : BoardShape = 
        let board = seq {
            yield! generateHeader size
            yield! generateBody size
            yield! generateFooter size
        }
        board
        |> Seq.mapi (fun i line -> 
            line |> Seq.mapi (fun j shape -> transform shape i j board size))

module HexagonCell =
    open Domain

    let isNeighbours reference other =
        match (other.ColumnNum - reference.ColumnNum, other.LineNum - reference.LineNum) with
        | (0, -2)
        | (0, 2)
        | (-1, -1)
        | (1, -1)
        | (1, 1)
        | (-1, 1) -> true
        | _ -> false

open Domain

let convertShapeToCells shape =
    let createCell lineNum columnNum isStartingPosition =
        { Id = { LineNum = lineNum + 1; ColumnNum = columnNum + 1 }; State = Free 0; IsStartingPosition = isStartingPosition }

    let convertCell lineNum columnNum = function
        | Cell -> createCell lineNum columnNum false |> Some
        | StartingCell -> createCell lineNum columnNum true |> Some
        | None -> Option.None

    let convertLine lineNum line =
        line 
        |> Seq.mapi (convertCell lineNum)
        |> Seq.choose id

    shape 
    |> Seq.mapi convertLine 
    |> Seq.collect id
