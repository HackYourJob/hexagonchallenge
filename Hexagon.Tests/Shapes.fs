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


module HexagonCell =
    open Hexagon.Domain
    open Hexagon.Shapes.HexagonCell
    
    let cell column line = { Id = sprintf "%i-%i" column line; LineNum = line; ColumnNum = column; State = Free 0 }
    let reference = cell 10 10
    
    type ``isNeighbours should`` ()=
        [<Fact>] member x.
            ``return true if top cell`` ()= 
            cell 10 8
            |> isNeighbours reference  
            |> should be True

        [<Fact>] member x.
            ``return true if bottom cell`` ()= 
            cell 10 12
            |> isNeighbours reference  
            |> should be True

        [<Fact>] member x.
            ``return true if right cell`` ()= 
            cell 12 10
            |> isNeighbours reference  
            |> should be True

        [<Fact>] member x.
            ``return true if left cell`` ()= 
            cell 8 10
            |> isNeighbours reference  
            |> should be True

        [<Fact>] member x.
            ``return true if top left cell`` ()= 
            cell 9 9
            |> isNeighbours reference  
            |> should be True

        [<Fact>] member x.
            ``return true if top right cell`` ()= 
            cell 11 9
            |> isNeighbours reference  
            |> should be True

        [<Fact>] member x.
            ``return true if bottom right cell`` ()= 
            cell 11 11
            |> isNeighbours reference  
            |> should be True

        [<Fact>] member x.
            ``return true if bottom left cell`` ()= 
            cell 9 11
            |> isNeighbours reference  
            |> should be True

        [<Fact>] member x.
            ``return false if too far`` ()= 
            cell 12 11
            |> isNeighbours reference  
            |> should be False



open Hexagon.Domain

type ``convertShapeToCell should`` ()=
    [<Fact>] member x.
        ``convert ShapeCell to Free Cell`` ()= 
        convertShapeToCells (fun _ -> "1") [ [Cell] ] 
        |> Seq.toList
        |> should equal [ { Id = "1"; LineNum = 1; ColumnNum = 1; State = Free 0 } ]

    [<Fact>] member x.
        ``exclude None`` ()= 
        convertShapeToCells (fun _ -> "1") [ [Cell; None] ] 
        |> Seq.toList
        |> should equal [ { Id = "1"; LineNum = 1; ColumnNum = 1; State = Free 0 } ]

    [<Fact>] member x.
        ``Fill good column num of line`` ()= 
        convertShapeToCells (fun _ -> "1") [ [Cell; Cell; Cell] ] 
        |> Seq.toList
        |> should equal 
            [ { Id = "1"; LineNum = 1; ColumnNum = 1; State = Free 0 } 
              { Id = "1"; LineNum = 1; ColumnNum = 2; State = Free 0 } 
              { Id = "1"; LineNum = 1; ColumnNum = 3; State = Free 0 } 
            ]

    [<Fact>] member x.
        ``Fill good line num`` ()= 
        convertShapeToCells (fun _ -> "1") [ [Cell]; [Cell]; [Cell] ] 
        |> Seq.toList
        |> should equal 
            [ { Id = "1"; LineNum = 1; ColumnNum = 1; State = Free 0 } 
              { Id = "1"; LineNum = 2; ColumnNum = 1; State = Free 0 } 
              { Id = "1"; LineNum = 3; ColumnNum = 1; State = Free 0 } 
            ]

    [<Fact>] 
    member x.``Use generateId for each cell`` ()= 
        let mutable cellCounter = 0
        convertShapeToCells (fun _ -> cellCounter <- cellCounter + 1; cellCounter |> string) [ [Cell; Cell; Cell] ] 
        |> Seq.toList
        |> should equal 
            [ { Id = "1"; LineNum = 1; ColumnNum = 1; State = Free 0 } 
              { Id = "2"; LineNum = 1; ColumnNum = 2; State = Free 0 } 
              { Id = "3"; LineNum = 1; ColumnNum = 3; State = Free 0 } 
            ]