//inspired from http://blog.tamizhvendan.in/blog/2015/06/11/building-rest-api-in-fsharp-using-suave/

open Suave.Web
open Suave.Successful
open HexagonRestApi.AisService
open HexagonRestApi.Rest.RestFul
open Domain


let usingInMemoryStorage = {
    GetAll = AiStorageInmemory.GetAll
    Exists = AiStorageInmemory.Exists
    Add = AiStorageInmemory.Add
    Update = AiStorageInmemory.Update
    GetById = AiStorageInmemory.GetById
    }

[<EntryPoint>]
let main argv =
  let aiWebPart = rest "ais" {
    GetAll = AisService.getAis usingInMemoryStorage
    Submit = AisService.submitAi usingInMemoryStorage
    GetById = AisService.getAi usingInMemoryStorage
   }
  startWebServer defaultConfig aiWebPart
  0
