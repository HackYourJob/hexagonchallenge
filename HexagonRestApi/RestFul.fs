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
open System.IO
open System.IO.Compression

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

let private encode (createStream : Stream * CompressionMode -> Stream) (bytes: byte[]) =
  if bytes.Length > 0 then
    use memory =  new MemoryStream()
    let compressStream = createStream(memory, CompressionMode.Compress)
    do compressStream.Write(bytes, 0, bytes.Length)
    compressStream.Dispose()
    memory.ToArray()
  else
    [||]
let private gzip (s:Stream, m:CompressionMode) = new GZipStream(s, m) :> Stream

let aiRest (submit: Ai -> unit) (tryToGetCode: Ai -> string option) (getMatchEvents: string -> string) (getTournamentNames: unit -> string[]) (getMatchsOfTournament: string -> Match[]) =    
    let errorIfNone = function
        | Some r -> r |> OK
        | _ -> NOT_FOUND "Resource not found"

    choose [
        path "/ais" >=> POST >=> request (getResourceFromRequest >> submit >> (fun () -> "Saved" |> OK))
        path "/ais/get" >=> POST >=> request (getResourceFromRequest >> tryToGetCode >> errorIfNone)
        pathScan "/matchs/%s/events" (fun (matchId) -> matchId |> getMatchEvents |> System.Text.Encoding.UTF8.GetBytes |> (encode gzip) |> ok) >=> Writers.setHeader "Content-Encoding" "gzip"
        path "/tournaments" >=> GET >=> (getTournamentNames () |> JSON)
        pathScan "/tournaments/%s/matchs" (fun (tournamentId) -> tournamentId |> getMatchsOfTournament |> JSON)
        ]
