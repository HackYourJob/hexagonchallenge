// Learn more about F# at http://fsharp.org
// See the 'F# Tutorial' project for more help.

open Suave
open Suave.Filters
open Suave.Operators
open System.IO

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

    start 8080 "../../../www"

    0 // return an integer exit code
