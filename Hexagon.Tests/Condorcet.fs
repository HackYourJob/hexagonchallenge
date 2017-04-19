module Hexagon.Tests.CondorcetTournament

open Xunit
open FsUnit.Xunit
open Swensen.Unquote

open HexagonTournament.Condorcet
open Hexagon.Domain
open Hexagon.BasicAi

type ``drawGames should`` () =
    let sixPlayers = [ 
        "BasicAi1" 
        "BasicAi2" 
        "BasicAi3" 
        "BasicAi4" 
        "BasicAi5" 
        "BasicAi6" ]
    let sixOtherPlayers = [ 
        "BasicAi7" 
        "BasicAi8" 
        "BasicAi9" 
        "BasicAi10" 
        "BasicAi11" 
        "BasicAi12" ]
    
    [<Fact>]
    member x.``return no games when no players`` ()=
        test <@ drawGames [] |> Seq.length = 0 @>
        
    [<Fact>]
    member x.``return a game when 6 players given randomly distributed`` ()=
        let games = drawGames sixPlayers
        
        (games |> Seq.head)
        |> Seq.mapi (fun i p -> sixPlayers |> Seq.item i = p)
        |> Seq.filter id
        |> Seq.length
        |> should lessThan 6
                
        (games |> Seq.head)
        |> Seq.iter (fun p -> sixPlayers |> should contain p)
            
    [<Fact>]
    member x.``return games with exactly 6 players`` ()=
        drawGames (sixPlayers @ sixOtherPlayers)
        |> Seq.iter (fun g -> g |> Seq.length |> should equal 6)
    
    [<Fact>]
    member x.``return games with same participation (half the nb of players) for each players`` ()=
        let games = drawGames (sixPlayers @ sixOtherPlayers)
        let nbGamePerPlayer =
            games |> Seq.collect id
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
        