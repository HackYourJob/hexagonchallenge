module Hexagon.Tests.Shapes

open System
open Xunit
open FsUnit.Xunit

open Hexagon.Shapes

let display shape =
    let serializeLine line =
        line 
        |> Seq.map (function StartingCell -> 'o' | Cell -> 'x' | None -> '.') 
        |> String.Concat

    shape 
    |> Seq.map serializeLine 
    |> Seq.toList

module HexagonBoard =
    open Hexagon.Shapes.HexagonBoard

    type ``generate should`` ()=
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
                 "x.x.o.x.x"]

        [<Fact>] member x.
         ``body has hexagon size and odd line`` ()= 
            generate 5 
            |> display
            |> Seq.skip 5
            |> Seq.take 7
            |> Seq.toList
            |> should equal 
                [".x.x.x.x."
                 "x.o.x.o.x"
                 ".x.x.x.x."
                 "x.x.x.x.x"
                 ".x.x.x.x."
                 "x.o.x.o.x"
                 ".x.x.x.x."]

        [<Fact>] member x.
         ``footer is inverse of header`` ()= 
            generate 5
            |> display 
            |> Seq.skip 5
            |> Seq.skip 7
            |> Seq.toList
            |> should equal 
                ["x.x.o.x.x"
                 ".x.x.x.x."
                 "..x.x.x.."
                 "...x.x..."
                 "....x...."]


module HexagonCell =
    open Hexagon.Domain
    open Hexagon.Shapes.HexagonCell
    
    let cellId column line = { LineNum = line; ColumnNum = column }
    let reference = cellId 10 10
    
    type ``isNeighbours should`` ()=
        [<Fact>] member x.
            ``return true if top cell`` ()= 
            cellId 10 8
            |> isNeighbours reference  
            |> should be True

        [<Fact>] member x.
            ``return true if bottom cell`` ()= 
            cellId 10 12
            |> isNeighbours reference  
            |> should be True

        [<Fact>] member x.
            ``return false if right cell`` ()= 
            cellId 12 10
            |> isNeighbours reference  
            |> should be False

        [<Fact>] member x.
            ``return false if left cell`` ()= 
            cellId 8 10
            |> isNeighbours reference  
            |> should be False

        [<Fact>] member x.
            ``return true if top left cell`` ()= 
            cellId 9 9
            |> isNeighbours reference  
            |> should be True

        [<Fact>] member x.
            ``return true if top right cell`` ()= 
            cellId 11 9
            |> isNeighbours reference  
            |> should be True

        [<Fact>] member x.
            ``return true if bottom right cell`` ()= 
            cellId 11 11
            |> isNeighbours reference  
            |> should be True

        [<Fact>] member x.
            ``return true if bottom left cell`` ()= 
            cellId 9 11
            |> isNeighbours reference  
            |> should be True

        [<Fact>] member x.
            ``return false if too far`` ()= 
            cellId 12 11
            |> isNeighbours reference  
            |> should be False



open Hexagon.Domain

type ``convertShapeToCell should`` ()=
    [<Fact>] member x.
        ``convert ShapeCell to Free Cell`` ()= 
        convertShapeToCells [ [Cell] ] 
        |> Seq.toList
        |> should equal [ { Id = { LineNum = 1; ColumnNum = 1 }; State = Free 0; IsStartingPosition = false } ]

    [<Fact>] member x.
        ``convert ShapeCell to Free starting Cell`` ()= 
        convertShapeToCells [ [StartingCell] ] 
        |> Seq.toList
        |> should equal [ { Id = { LineNum = 1; ColumnNum = 1 }; State = Free 0; IsStartingPosition = true } ]

    [<Fact>] member x.
        ``exclude None`` ()= 
        convertShapeToCells [ [Cell; None] ] 
        |> Seq.toList
        |> should equal [ { Id = { LineNum = 1; ColumnNum = 1 }; State = Free 0; IsStartingPosition = false } ]

    [<Fact>] member x.
        ``Fill good column num of line`` ()= 
        convertShapeToCells [ [Cell; Cell; Cell] ] 
        |> Seq.toList
        |> should equal 
            [ { Id = { LineNum = 1; ColumnNum = 1 }; State = Free 0; IsStartingPosition = false } 
              { Id = { LineNum = 1; ColumnNum = 2 }; State = Free 0; IsStartingPosition = false } 
              { Id = { LineNum = 1; ColumnNum = 3 }; State = Free 0; IsStartingPosition = false } 
            ]

    [<Fact>] member x.
        ``Fill good line num`` ()= 
        convertShapeToCells [ [Cell]; [Cell]; [Cell] ] 
        |> Seq.toList
        |> should equal 
            [ { Id = { LineNum = 1; ColumnNum = 1 }; State = Free 0; IsStartingPosition = false } 
              { Id = { LineNum = 2; ColumnNum = 1 }; State = Free 0; IsStartingPosition = false } 
              { Id = { LineNum = 3; ColumnNum = 1 }; State = Free 0; IsStartingPosition = false } 
            ]