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
    
open System.Reactive.Subjects   
open System.Reactive.Concurrency
open FSharp.Control.Reactive.Observable
open System.Reactive.Linq
open FSharp.Control.Reactive

let sendObject send data =
    data
    |> Compact.serialize
    |> Encoding.UTF8.GetBytes
    |> send Text

let logIfErrorOnSend send =
    async {
        let! result = send
        match result with
        | Choice1Of2 _ -> ()
        | Choice2Of2 error ->
            printfn "%A" error
            ()
    }

type EventsPublisher (publish)=
    let cts = new System.Threading.CancellationTokenSource()
    let mailboxProcess (inbox: MailboxProcessor<_>) =
        let rec loop() = async{
            let! msg = inbox.Receive()

            do! msg |> publish
                
            return! loop()  
            }
            
        loop()
    let messagesQueue = MailboxProcessor.Start(mailboxProcess, cancellationToken = cts.Token)

    let obs = new Subject<Hexagon.Domain.GameEvents>()
    let subscribe =
        obs 
        |> Observable.zip (Observable.interval (TimeSpan.FromMilliseconds 1.))
        |> Observable.map (fun (_, evt) -> evt)
        |> Observable.bufferSpanCount (TimeSpan.FromMilliseconds 50.) 10
        |> Observable.subscribe messagesQueue.Post

    member x.publish evt =
        if cts.IsCancellationRequested |> not
        then
            obs.OnNext evt

    interface IDisposable with
        member x.Dispose() = 
          cts.Cancel()
          (obs :> IDisposable).Dispose()
          subscribe.Dispose()
          (messagesQueue :> IDisposable).Dispose()

let wsServer (webSocket : WebSocket) =
    let send opcode data =
        webSocket.send opcode data true

    let publisher = new EventsPublisher(fun msg -> sendObject send msg |> logIfErrorOnSend)
    let cts = new System.Threading.CancellationTokenSource()

    fun cx -> socket {
        let loop = ref true
        while !loop do
            let! msg = webSocket.read()
            match msg with
            | (Text, data, true) ->
                async {
                    Hexagon.Game.startGame publisher.publish cts.Token 10
                } |> Async.Start
            | (Ping, _, _) ->
                do! send Pong [||]
            | (Close, _, _) ->
                do! send Close [||]
                cts.Cancel()
                (publisher :> IDisposable).Dispose()
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
        path "/websocket" >=> handShake wsServer
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
