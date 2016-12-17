module Hexagon.Tests.Hexagon

open System
open Xunit
open FsUnit.Xunit

open Hexagon.Hexagon

let display hexagon =
    let serializeLine line =
        line 
        |> Seq.map (function Cell -> 'x' | None -> '.') 
        |> String.Concat

    hexagon 
    |> Seq.map serializeLine 
    |> Seq.toList

type ``generateHexagon should`` ()=
    [<Fact>] member x.
     ``center cell on first line`` ()= 
        generateHexagon 5 
        |> display
        |> Seq.head
        |> should equal "....x...."

    [<Fact>] member x.
     ``two cells on second line`` ()= 
        generateHexagon 5 
        |> display
        |> Seq.skip 1
        |> Seq.head
        |> should equal "...x.x..."

    [<Fact>] member x.
     ``header is arithmetic sequence of growth 2 with only pair value`` ()= 
        generateHexagon 5 
        |> display
        |> Seq.take 5
        |> Seq.toList
        |> should equal 
            ["....x...."
             "...x.x..."
             "..x.x.x.."
             ".x.x.x.x."
             "x.x.x.x.x"]

    [<Fact>] member x.
     ``body has hexagon size`` ()= 
        generateHexagon 5 
        |> display
        |> Seq.skip 4
        |> Seq.take 5
        |> Seq.toList
        |> should equal 
            ["x.x.x.x.x"
             "x.x.x.x.x"
             "x.x.x.x.x"
             "x.x.x.x.x"
             "x.x.x.x.x"]

    [<Fact>] member x.
     ``footer is inverse of header`` ()= 
        generateHexagon 5
        |> display 
        |> Seq.skip 4
        |> Seq.skip 4
        |> Seq.toList
        |> should equal 
            ["x.x.x.x.x"
             ".x.x.x.x."
             "..x.x.x.."
             "...x.x..."
             "....x...."]
