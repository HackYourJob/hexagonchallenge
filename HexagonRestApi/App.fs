//inspired from http://blog.tamizhvendan.in/blog/2015/06/11/building-rest-api-in-fsharp-using-suave/

open Suave.Web
open Suave.Successful
open HexagonRestApi.Rest
open HexagonRestApi.Storage


[<EntryPoint>]
let main argv =
  let aiWebPart = rest "ais" {
    GetAll = Storage.getAis
  }
  startWebServer defaultConfig aiWebPart
  0
