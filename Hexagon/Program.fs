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

let start port wwwDirectory =

    let config = 
        { defaultConfig with 
                bindings = [ HttpBinding.mkSimple HTTP "0.0.0.0" port ]
                homeFolder = Some (Path.GetFullPath wwwDirectory) }

    let app : WebPart =
      choose [
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
