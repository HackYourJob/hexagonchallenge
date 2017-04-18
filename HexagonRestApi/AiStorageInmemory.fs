module AiStorageInmemory

open System.Collections.Generic
open Domain

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
