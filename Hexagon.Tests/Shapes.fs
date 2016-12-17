module Hexagon.Tests.Shapes

open System
open Xunit
open FsUnit.Xunit

open Hexagon.Shapes

let display shape =
    let serializeLine line =
        line 
        |> Seq.map (function Cell -> 'x' | None -> '.') 
        |> String.Concat

    shape 
    |> Seq.map serializeLine 
    |> Seq.toList

module HexagonBoard =
    open Hexagon.Shapes.HexagonBoard

    type ``generateHexagon should`` ()=
        [<Fact>] member x.
         ``center cell on first line`` ()= 
            generate 5 
            |> display
            |> Seq.head
            |> should equal "....x...."

        [<Fact>] member x.
         ``two cells on second line`` ()= 
            generate 5 
            |> display
            |> Seq.skip 1
            |> Seq.head
            |> should equal "...x.x..."

        [<Fact>] member x.
         ``header is arithmetic sequence of growth 2 with only pair value`` ()= 
            generate 5 
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
            generate 5 
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
            generate 5
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
