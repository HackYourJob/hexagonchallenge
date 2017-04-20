module HexagonRestApi.RestFul

open Newtonsoft.Json
open Newtonsoft.Json.Serialization
open Suave.Successful
open Suave
open Suave.Operators 
open Suave.Filters
open Suave.Successful
open Suave.RequestErrors
open HexagonRestApi.AisService
open HexagonRestApi.Domain

let JSON objectToSerialize =
    let jsonSerializerSettings = new JsonSerializerSettings()
    jsonSerializerSettings.ContractResolver <- new CamelCasePropertyNamesContractResolver()
    JsonConvert.SerializeObject(objectToSerialize, jsonSerializerSettings) 
    |> OK
    >=> Writers.setMimeType("application/json; charset=utf-8")

let fromJson<'a> json =
    JsonConvert.DeserializeObject(json, typeof<'a>) :?> 'a

let getResourceFromRequest<'a> (req : HttpRequest) =
    let getString rawForm =
        System.Text.Encoding.UTF8.GetString(rawForm)
    req.rawForm |> getString |> fromJson<'a>


let aiRest (submit: Ai -> unit) (tryToGetCode: Ai -> string option) (getMatchEvents: string -> string) =    
    let errorIfNone = function
        | Some r -> r |> OK
        | _ -> NOT_FOUND "Resource not found"

    choose [
        path "/ais" >=> POST >=> request (getResourceFromRequest >> submit >> (fun () -> "Saved" |> OK))
        path "/ais/get"  >=> POST >=> request (getResourceFromRequest >> tryToGetCode >> errorIfNone)
        pathScan "/matchs/%s/events" (fun (matchId) -> matchId |> getMatchEvents |> OK)
        ]
