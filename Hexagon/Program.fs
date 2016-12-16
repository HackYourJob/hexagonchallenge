// Learn more about F# at http://fsharp.org
// See the 'F# Tutorial' project for more help.


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

let echo (webSocket : WebSocket) =
  fun cx -> socket {
    let loop = ref true
    while !loop do
      let! msg = webSocket.read()
      match msg with
      | (Text, data, true) ->
        let str = Encoding.UTF8.GetString data
        do! webSocket.send Text data true
      | (Ping, _, _) ->
        do! webSocket.send Pong [||] true
      | (Close, _, _) ->
        do! webSocket.send Close [||] true
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
        path "/websocket" >=> handShake echo
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
