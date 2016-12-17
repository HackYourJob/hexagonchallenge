module Hexagon.Hexagon

type GeneratedCell = Cell | None

let none _ = None
let oddSeq i = if i%2 = 0 then None else Cell

let generateLine size num =
    seq {
        yield! [1 .. size-num] |> Seq.map none
        yield! [1 .. (1 + (num - 1) * 2)] |> Seq.map oddSeq
        yield! [1 .. size-num] |> Seq.map none 
    }

let generateHeader size =
    [1 .. size - 1] |> Seq.map (generateLine size)

let generateFooter = generateHeader >> Seq.rev

let generateBody size =
    let fullLine _ = generateLine size size

    [1 .. size] |> Seq.map fullLine

let generateHexagon size = 
    seq {
        yield! generateHeader size
        yield! generateBody size
        yield! generateFooter size
    }