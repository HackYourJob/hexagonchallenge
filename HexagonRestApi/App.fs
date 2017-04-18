//inspired from http://blog.tamizhvendan.in/blog/2015/06/11/building-rest-api-in-fsharp-using-suave/

open Suave.Web
open Suave.Successful
open HexagonRestApi.AisStorage
open HexagonRestApi.Rest.RestFul


[<EntryPoint>]
let main argv =
  let aiWebPart = rest "ais" {
    GetAll = AisStorage.getAis
    Submit = AisStorage.submitAi
    GetById = AisStorage.getAi
   }
  startWebServer defaultConfig aiWebPart
  0
