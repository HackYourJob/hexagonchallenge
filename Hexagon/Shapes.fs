module Hexagon.Shapes

type ShapeCell = Cell | None
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
        let fullLine _ = generateLine size size

        [1 .. size] |> Seq.map fullLine

    let generate (size: int) : BoardShape = 
        seq {
            yield! generateHeader size
            yield! generateBody size
            yield! generateFooter size
        }

module HexagonCell =
    open Domain

    let isNeighbours reference other =
        match (other.ColumnNum - reference.ColumnNum, other.LineNum - reference.LineNum) with
        | (0, -2)
        | (0, 2)
        | (-2, 0)
        | (2, 0)
        | (-1, -1)
        | (1, -1)
        | (1, 1)
        | (-1, 1) -> true
        | _ -> false

open Domain

let convertShapeToCells shape =
    let createCell lineNum columnNum =
        { Id = { LineNum = lineNum + 1; ColumnNum = columnNum + 1 }; State = Free 0 }

    let convertCell lineNum columnNum = function
        | Cell -> createCell lineNum columnNum |> Some
        | None -> Option.None

    let convertLine lineNum line =
        line 
        |> Seq.mapi (convertCell lineNum)

    shape 
    |> Seq.mapi convertLine 
    |> Seq.collect id
    |> Seq.filter Option.isSome
    |> Seq.map Option.get