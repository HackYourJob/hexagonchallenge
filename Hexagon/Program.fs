open System
open System.IO
open System.Net
open System.Text

open Suave
open Suave.Web
open Suave.Successful
open Suave.Http
open Suave.Operators
open Suave.Filters
open Suave.Successful
open Suave.Files
open Suave.RequestErrors
open Suave.Logging
open Suave.Utils
open HexagonRestApi
open HexagonRestApi.Rest.RestFul
open HexagonRestApi.Domain.Domain


//let usingInMemoryStorage = {
//    GetAll = AiStorageInmemory.GetAll
//    Exists = AiStorageInmemory.Exists
//    Add = AiStorageInmemory.Add
//    Update = AiStorageInmemory.Update
//    GetById = AiStorageInmemory.GetById
//    }

//let usingInAzureDb = {
//    GetAll = AiStorageInDocumentDb.GetAll
//    Exists = AiStorageInDocumentDb.Exists
//    Add = AiStorageInDocumentDb.Add
//    Update = AiStorageInDocumentDb.Update
//    GetById = AiStorageInDocumentDb.GetById
//    }

let usingMySqlStorage = {
    GetAll = AiStorageInMySql.getAll
    Exists = AiStorageInMySql.exists
    Add = AiStorageInMySql.add
    Update = AiStorageInMySql.update
    GetById = AiStorageInMySql.getById
}

let start port wwwDirectory =

    let config = 
        { defaultConfig with 
                bindings = [ HttpBinding.mkSimple HTTP "0.0.0.0" port ]
                homeFolder = Some (Path.GetFullPath wwwDirectory) }
   
    let aiRestWebPart = rest "ais" {
        Submit = AisService.submitAi usingMySqlStorage
        GetById = AisService.getAi usingMySqlStorage
    }

    let app : WebPart =
      choose [
        GET >=> path "/" >=> Files.browseFileHome "index.html"
        GET >=> Files.browseHome
        aiRestWebPart
        RequestErrors.NOT_FOUND "Page not found." 
      ]

    startWebServer config app





[<EntryPoint>]
let main argv = 
    printfn "%A" argv

    start (int argv.[0]) argv.[1]

    0 // return an integer exit code
