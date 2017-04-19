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
        
        (games |> Seq.head)
        |> Seq.mapi (fun i p -> sixPlayers |> Seq.item i |> fst = fst p)
        |> Seq.filter id
        |> Seq.length
        |> should lessThan 6
                
        (games |> Seq.head)
        |> Seq.iter (fun p -> sixPlayers |> Seq.map fst |> should contain (fst p))

    [<Fact>]
    member x.``return a game with additional players when strictly less than 6 players given`` ()=
        let firstGame = drawTournament (sixPlayers |> List.tail) |> Seq.head
        firstGame
        |> Seq.map (fun p -> (fst p).Id)
        |> Seq.distinct 
        |> Seq.length
        |> should equal 6
    
    [<Fact>]
    member x.``return games with exactly 6 players`` ()=
        drawTournament (sixPlayers @ sixOtherPlayers)
        |> Seq.iter (fun g -> g |> Seq.length |> should equal 6)
    
    [<Fact>]
    member x.``return games with same participation (half the nb of players) for each players`` ()=
        let games = drawTournament (sixPlayers @ sixOtherPlayers)
        let nbGamePerPlayer =
            games |> Seq.collect id
            |> Seq.map (fun p -> (fst p).Id)
            |> Seq.countBy id
        nbGamePerPlayer
        |> Seq.iter (fun (x, y) -> y |> should equal 6)
        
type ``determine best players should`` () =
    let player1 = { Id = 1; Name = "Player 1" }
    let player2 = { Id = 2; Name = "Player 2" }
    let player3 = { Id = 3; Name = "Player 3" }
    let gameRanking1 = [ player1; player2; player3 ] 
    let gameRanking2 = [ player2; player3; player1 ]
    let gameRanking3 = [ player3; player1; player2 ]
    
    [<Fact>]
    member x.``return players in same order given same game ranking passed`` ()=
        let gamesRankings = [ gameRanking1; gameRanking1 ]
        let bestPlayers = [ 
            { Ai = player1; NbDuelWon = 2; DuelWonBalance = 4 }
            { Ai = player2; NbDuelWon = 1; DuelWonBalance = 0 }
            { Ai = player3; NbDuelWon = 0; DuelWonBalance = -4 }]
        test <@ determineBestPlayers gamesRankings |> Seq.toList = bestPlayers @>

    [<Fact>]
    member x.``return in first position players having more won games against others`` ()=
        let gamesRankings = [ gameRanking1; gameRanking2; gameRanking2 ]
        let bestPlayers = [ 
            { Ai = player2; NbDuelWon = 2; DuelWonBalance = 4 }
            { Ai = player3; NbDuelWon = 1; DuelWonBalance = -2 }
            { Ai = player1; NbDuelWon = 0; DuelWonBalance = -2 } ]
        test <@ determineBestPlayers gamesRankings |> Seq.toList = bestPlayers @>
        
    [<Fact>]
    member x.``return in first position players having more won duels against others, then having won more games`` ()=
        let gamesRankings = [ gameRanking1; gameRanking3 ]
        let bestPlayers = [ 
            { Ai = player1; NbDuelWon = 1; DuelWonBalance = 2 }
            { Ai = player3; NbDuelWon = 0; DuelWonBalance = 0 }
            { Ai = player2; NbDuelWon = 0; DuelWonBalance = -2 } ]
        test <@ determineBestPlayers gamesRankings |> Seq.toList = bestPlayers @>
        