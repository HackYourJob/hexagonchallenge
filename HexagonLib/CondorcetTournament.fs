module Hexagon.CondorcetTournament

open Hexagon.Domain
open Hexagon.Ais

type Player = AiDescription * (AiCell [] -> TransactionParameters option) 

type Game = {
    Players: Player seq
}

let drawTournament (players: Player list) = 
    if players |> Seq.length = 0 then List.empty<Game>
    else 
        let nbPlayers = players |> Seq.length
        let lastPlayerId = players |> Seq.rev |> Seq.head |> fst |> fun p -> p.Id
        let basicAis = 
            [nbPlayers + 1..6]
            |> List.map (fun i -> { Id = i + lastPlayerId; Name = sprintf "Basic AI %i" i }, Hexagon.BasicAi.play)
        let players = players @ basicAis
        let random = new System.Random()
        let rand = fun () -> random.Next()
        let randomizedPlayers = 
            players
            |> Seq.map (fun p -> p, rand())
            |> Seq.sortBy snd
            |> Seq.map fst
        [{ Players = randomizedPlayers }]