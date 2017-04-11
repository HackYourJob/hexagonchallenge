module Hexagon.Tests.CondorcetTournament

open Xunit
open FsUnit.Xunit

open Hexagon.CondorcetTournament
open Hexagon.Domain
open Hexagon.BasicAi

type ``drawGames should`` () =
    let sixPlayers = [ 
        { Id = 1; Name = "BasicAi1" }, play 
        { Id = 2; Name = "BasicAi2" }, play 
        { Id = 3; Name = "BasicAi3" }, play 
        { Id = 4; Name = "BasicAi4" }, play 
        { Id = 5; Name = "BasicAi5" }, play 
        { Id = 6; Name = "BasicAi6" }, play ]
    
    [<Fact>]
    member x.``return no game when no AI`` ()=
        drawTournament []
        |> Seq.length
        |> should equal 0
        
    [<Fact>]
    member x.``return one game when 6 AI given randomly distributed`` ()=
        let games = drawTournament sixPlayers
        games |> should haveLength 1
        
        (games |> Seq.head).Players 
        |> Seq.mapi (fun i p -> sixPlayers |> Seq.item i |> fst = fst p)
        |> Seq.filter id
        |> Seq.length
        |> should lessThan 6
                
        (games |> Seq.head).Players
        |> Seq.iter (fun p -> sixPlayers |> Seq.map fst |> should contain (fst p))

    [<Fact>]
    member x.``return game with additional AIs when strictly less than 6 AI given`` ()=
        let firstGame = drawTournament (sixPlayers |> List.tail) |> Seq.head
        firstGame.Players 
        |> Seq.map (fun p -> (fst p).Id)
        |> Seq.distinct 
        |> Seq.length
        |> should equal 6