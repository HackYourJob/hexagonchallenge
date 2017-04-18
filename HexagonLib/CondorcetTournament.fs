module Hexagon.CondorcetTournament

open Hexagon.Domain
open Hexagon.Ais

type Player = AiDescription * (AiCell [] -> TransactionParameters option) 

type Game = {
    Players: Player seq
}

type GameRanking = {
    PlayersFromFirstToLast: AiDescription seq
}

let private appendPlayersIfNeeded (players: Player list) =
    let nbPlayers = players |> Seq.length
    let lastPlayerId = players |> Seq.rev |> Seq.head |> fst |> fun p -> p.Id
    let basicAis = 
        [nbPlayers + 1..6]
        |> List.map (fun i -> { Id = i + lastPlayerId; Name = sprintf "Basic AI %i" i }, Hexagon.BasicAi.play)
    players @ basicAis

let private splitInGames players =
    let rec loop xs =
        [
            yield xs |> List.take 6
            match List.length xs > 6 with
            | true -> yield! xs |> List.skip 6 |> loop
            | false -> ()
        ]
    loop players
    |> List.map (fun players -> { Players = players })

let private games rand players =
    let games =
        players
        |> Seq.map (fun p -> p, rand())
        |> Seq.sortBy snd
        |> Seq.map fst
        |> Seq.toList
        |> splitInGames
    games

let drawTournament players = 
    if players |> Seq.length = 0 then List.empty<Game>
    else 
        
        let random = new System.Random()
        let rand = fun () -> random.Next()
        
        let players =
            players
            |> appendPlayersIfNeeded
        
        [1..(players |> Seq.length) / 2]
        |> List.collect (fun _ -> games rand players)

type CondorcetGraph = Map<AiDescription, Map<AiDescription, int>>
type DuelResult = WinAgainst | LooseAgainst
type Score = { Ai: AiDescription; NbDuelWon: int; DuelWonBalance: int }

let private updateCondorcetGraph' playerA duelResult playerB graph =
    let mutable nbDuelWon = match duelResult with WinAgainst -> 1 | LooseAgainst -> -1
    if graph |> Map.containsKey playerA then
        let existingPlayerA = graph |> Map.find playerA
        if existingPlayerA |> Map.containsKey playerB then
            nbDuelWon <- nbDuelWon + (existingPlayerA |> Map.find playerB)
            let newPlayerA = 
                existingPlayerA |> Map.remove playerB
                |> Map.add playerB nbDuelWon
            graph |> Map.remove playerA
            |> Map.add playerA newPlayerA
        else
            graph |> Map.remove playerA
            |> Map.add playerA (existingPlayerA |> Map.add playerB nbDuelWon)
    else
        graph |> Map.add playerA (Map.ofList [ playerB, nbDuelWon ])

let private updateCondorcetGraph (condorcetGraph:CondorcetGraph) (gameRanking:AiDescription seq) =
    gameRanking
    |> Seq.tail
    |> Seq.mapi (fun i x -> [0..i] |> Seq.map (fun j -> gameRanking |> Seq.item j, x))
    |> Seq.collect id
    |> Seq.fold (fun graph duel -> 
        let duelWinner, duelLooser = duel
        graph
        |> updateCondorcetGraph' duelWinner WinAgainst duelLooser
        |> updateCondorcetGraph' duelLooser LooseAgainst duelWinner 
        ) condorcetGraph

let determineBestPlayers gamesRankings =
    let bestPlayersUnsorted =
        gamesRankings
        |> Seq.map (fun x -> x.PlayersFromFirstToLast)
        |> Seq.fold updateCondorcetGraph Map.empty<AiDescription, Map<AiDescription, int>>
        |> Map.toSeq
    bestPlayersUnsorted
    |> Seq.map (fun x -> { Ai = fst x;
                        NbDuelWon = snd x |> Map.toSeq |> Seq.filter (fun x -> snd x > 0) |> Seq.length;
                        DuelWonBalance = snd x |> Map.toSeq |> Seq.sumBy (fun x -> snd x) })
    |> Seq.sortByDescending (fun x -> x.NbDuelWon)
    |> Seq.sortByDescending (fun x -> x.DuelWonBalance)
