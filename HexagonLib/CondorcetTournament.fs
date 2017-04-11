module Hexagon.CondorcetTournament

open Hexagon.Domain
open Hexagon.Ais

type Player = AiDescription * (AiCell [] -> TransactionParameters option) 

type Game = {
    Players: Player seq
}

let private appendPlayersIfNeeded (players: Player list) =
    let nbPlayers = players |> Seq.length
    let lastPlayerId = players |> Seq.rev |> Seq.head |> fst |> fun p -> p.Id
    let basicAis = 
        [nbPlayers + 1..6]
        |> List.map (fun i -> { Id = i + lastPlayerId; Name = sprintf "Basic AI %i" i }, Hexagon.BasicAi.play)
    players @ basicAis

let splitInGames players =
    let rec loop xs =
        [
            yield xs |> Seq.take 6
            match Seq.length xs > 6 with
            | true -> yield! xs |> Seq.skip 6 |> loop
            | false -> ()
        ]
    loop players
    |> Seq.map (fun players -> { Players = players })

let drawTournament players = 
    if players |> Seq.length = 0 then List.empty<Game>
    else 
        
        let random = new System.Random()
        let rand = fun () -> random.Next()
        
        let players =
            players
            |> appendPlayersIfNeeded
        
        players
        |> Seq.map (fun p -> p, rand())
        |> Seq.sortBy snd
        |> Seq.map fst
        |> splitInGames
        |> Seq.toList
