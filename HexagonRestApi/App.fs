//inspired from http://blog.tamizhvendan.in/blog/2015/06/11/building-rest-api-in-fsharp-using-suave/

open Suave.Web
open Suave.Successful

[<EntryPoint>]
let main argv =
  startWebServer defaultConfig (OK "Hello, Suave!")
  0
