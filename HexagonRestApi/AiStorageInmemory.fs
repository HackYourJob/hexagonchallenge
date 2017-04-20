module AiStorageInmemory

open System.Collections.Generic
open HexagonRestApi.Domain

let private aiMemoryStorage = new Dictionary<string, Ai>()

let GetAll () = aiMemoryStorage.Values |> Seq.map (fun ai -> ai)
    
let GetById id = aiMemoryStorage.[id]

let Update (id,ai) = 
    aiMemoryStorage.[id] <- ai
    ai

let Exists id = aiMemoryStorage.ContainsKey id

let Add (id,ai) = 
    aiMemoryStorage.Add(id, ai)
    ai

let tryToGetCode id ai =
    match aiMemoryStorage.ContainsKey id with
    | true -> Some aiMemoryStorage.[id].Content
    | false -> None

let updateOrAdd id ai =
    match aiMemoryStorage.ContainsKey id with
    | false -> aiMemoryStorage.Add(id, ai)
    | true -> aiMemoryStorage.[id] <- ai

let getMatchEvents matchId =
    "fdsfsdfsdfsf " + matchId

let getTournamentNames () = [|"Esai1"; "Essai2"|] 
let getMatchsOfTournament tournamentId =
    [|
        { Id = "M1"; Date = System.DateTime.Now; AiNames = [|"Joe"; "Bob"|]} 
        { Id = "M2"; Date = System.DateTime.Now; AiNames = [|"Joe2"; "Bob2"|]} 
    |]