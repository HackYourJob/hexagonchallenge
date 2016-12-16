open System
open System.IO
open System.Net
open System.Text

open Suave
open Suave.Http
open Suave.Operators
open Suave.Filters
open Suave.Successful
open Suave.Files
open Suave.RequestErrors
open Suave.Logging
open Suave.Utils
open Suave.Sockets
open Suave.Sockets.Control
open Suave.WebSocket

open Microsoft.FSharpLu.Json

type ClientCommands =
    | Echo of string

type ClientCommandResponses =
    | Echo of string

let applyCommand = function
    | ClientCommands.Echo m -> ClientCommandResponses.Echo ("WS:" + m)

let echo (applyCommand: ClientCommands -> ClientCommandResponses) (webSocket : WebSocket) =
    let send opcode data =
        webSocket.send opcode data true

    fun cx -> socket {
        let loop = ref true
        while !loop do
            let! msg = webSocket.read()
            match msg with
            | (Text, data, true) ->
                do! data
                    |> Encoding.UTF8.GetString
                    |> Compact.deserialize<ClientCommands>
                    |> applyCommand
                    |> Compact.serialize
                    |> Encoding.UTF8.GetBytes
                    |> send Text
            | (Ping, _, _) ->
                do! send Pong [||]
            | (Close, _, _) ->
                do! send Close [||]
                loop := false
            | _ -> ()
    }

let start port wwwDirectory =

    let config = 
        { defaultConfig with 
                bindings = [ HttpBinding.mkSimple HTTP "0.0.0.0" port ]
                homeFolder = Some (Path.GetFullPath wwwDirectory) }

    let app : WebPart =
      choose [
        path "/websocket" >=> handShake (echo applyCommand)
        GET >=> path "/" >=> Files.browseFileHome "index.html"
        GET >=> Files.browseHome
        RequestErrors.NOT_FOUND "Page not found." 
      ]

    startWebServer config app

[<EntryPoint>]
let main argv = 
    printfn "%A" argv

    start (int argv.[0]) argv.[1]

    0 // return an integer exit code
