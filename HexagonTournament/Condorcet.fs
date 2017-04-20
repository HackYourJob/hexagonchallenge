module HexagonTournament.Condorcet

open Hexagon.Domain
open Hexagon.Ais
open System

type PlayerId = string

type GamePlayers = PlayerId list

type AiId = string

type GameAiScore = {
    AiId:AiId
    AiName: string
    Resources: int
    Cells: int
    Bugs: int
}

type GameRankingFromFirstToLast = GameAiScore list

let private splitInGames players =
    let rec loop xs =
        [
            yield xs |> List.take 6
            match List.length xs > 6 with
            | true -> yield! xs |> List.skip 6 |> loop
            | false -> ()
        ]
    loop players

let private games rand players =
    let games =
        players
        |> Seq.map (fun p -> p, rand())
        |> Seq.sortBy snd
        |> Seq.map fst
        |> Seq.toList
        |> splitInGames
    games

let drawGames players = 
    if players |> Seq.length = 0 then List.empty<GamePlayers>
    else 
        let random = new System.Random()
        let rand = fun () -> random.Next()
                
        [1..(players |> Seq.length) / 2]
        |> List.collect (fun _ -> games rand players)

type CondorcetGraph = Map<AiId, Map<AiId, int>>
type DuelResult = WinAgainst | LooseAgainst
type Score = { AiScore: GameAiScore; NbDuelWon: int; DuelWonBalance: int }

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

let private updateCondorcetGraph (condorcetGraph:CondorcetGraph) (gameRanking:GameRankingFromFirstToLast) =
    gameRanking
    |> Seq.tail
    |> Seq.mapi (fun i x -> [0..i] |> Seq.map (fun j -> (gameRanking |> Seq.item j).AiId, x.AiId))
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
        |> Seq.fold updateCondorcetGraph Map.empty<AiId, Map<AiId, int>>
        |> Map.toSeq
    let playersTotalScore =
        gamesRankings
        |> Seq.collect id
        |> Seq.groupBy (fun x -> x.AiId)
        |> Seq.map (fun (x,y) -> 
            x, 
            { 
                AiId = x
                AiName = (y |> Seq.head).AiName
                Cells = y |> Seq.sumBy (fun z -> z.Cells)
                Resources = y |> Seq.sumBy (fun z -> z.Resources)
                Bugs = y |> Seq.sumBy (fun z -> z.Bugs)
            })
        |> Map.ofSeq
    bestPlayersUnsorted
    |> Seq.map (fun x -> { AiScore = playersTotalScore |> Map.find (fst x);
                        NbDuelWon = snd x |> Map.toSeq |> Seq.filter (fun x -> snd x > 0) |> Seq.length;
                        DuelWonBalance = snd x |> Map.toSeq |> Seq.sumBy (fun x -> snd x) })
    |> Seq.sortByDescending (fun x -> 
        x.NbDuelWon, 
        x.DuelWonBalance, 
        -x.AiScore.Bugs, 
        x.AiScore.Cells, 
        x.AiScore.Resources)
