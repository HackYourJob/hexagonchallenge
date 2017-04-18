module Hexagon.Tests.CondorcetTournament

open Xunit
open FsUnit.Xunit
open Swensen.Unquote

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
    let sixOtherPlayers = [ 
        { Id = 7; Name = "BasicAi7" }, play 
        { Id = 8; Name = "BasicAi8" }, play 
        { Id = 9; Name = "BasicAi9" }, play 
        { Id = 10; Name = "BasicAi10" }, play 
        { Id = 11; Name = "BasicAi11" }, play 
        { Id = 12; Name = "BasicAi12" }, play ]
    
    [<Fact>]
    member x.``return no games when no players`` ()=
        test <@ drawTournament [] |> Seq.length = 0 @>
        
    [<Fact>]
    member x.``return a game when 6 players given randomly distributed`` ()=
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
    member x.``return a game with additional players when strictly less than 6 players given`` ()=
        let firstGame = drawTournament (sixPlayers |> List.tail) |> Seq.head
        firstGame.Players 
        |> Seq.map (fun p -> (fst p).Id)
        |> Seq.distinct 
        |> Seq.length
        |> should equal 6
    
    [<Fact>]
    member x.``return games with exactly 6 players`` ()=
        drawTournament (sixPlayers @ sixOtherPlayers)
        |> Seq.iter (fun g -> g.Players |> Seq.length |> should equal 6)
    
    [<Fact>]
    member x.``return games with same participation (half the nb of players) for each players`` ()=
        let games = drawTournament (sixPlayers @ sixOtherPlayers)
        let nbGamePerPlayer =
            games |> Seq.collect (fun g -> g.Players)
            |> Seq.map (fun p -> (fst p).Id)
            |> Seq.countBy id
        nbGamePerPlayer
        |> Seq.iter (fun (x, y) -> y |> should equal 6)
        