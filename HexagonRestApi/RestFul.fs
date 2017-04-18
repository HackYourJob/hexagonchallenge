namespace HexagonRestApi.Rest
[<AutoOpen>]
module RestFul =
  open Newtonsoft.Json
  open Newtonsoft.Json.Serialization
  open Suave.Successful
  open Suave
  open Suave.Operators 
  open Suave.Filters
  open Suave.Successful
    
  type RestResource<'a> = {
    GetAll : unit -> 'a seq
  }


  let JSON objectToSerialize =
    let jsonSerializerSettings = new JsonSerializerSettings()
    jsonSerializerSettings.ContractResolver <- new CamelCasePropertyNamesContractResolver()
    JsonConvert.SerializeObject(objectToSerialize, jsonSerializerSettings) 
    |> OK
    >=> Writers.setMimeType("application/json; charset=utf-8")

  let rest resourceName resource =
    let resourcePath = "/" + resourceName
    let getAll = warbler (fun _ -> resource.GetAll () |> JSON)
    path resourcePath >=> GET >=> getAll

