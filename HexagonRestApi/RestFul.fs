namespace HexagonRestApi.Rest

module RestFul =
  open Newtonsoft.Json
  open Newtonsoft.Json.Serialization
  open Suave.Successful
  open Suave
  open Suave.Operators 
  open Suave.Filters
  open Suave.Successful
  open Suave.RequestErrors
  open HexagonRestApi.Storage
    
  type RestResource<'a> = {
    GetAll : unit -> 'a seq
    Create : 'a -> 'a
    GetById : 'a -> 'a option
  }

  let JSON objectToSerialize =
    let jsonSerializerSettings = new JsonSerializerSettings()
    jsonSerializerSettings.ContractResolver <- new CamelCasePropertyNamesContractResolver()
    JsonConvert.SerializeObject(objectToSerialize, jsonSerializerSettings) 
    |> OK
    >=> Writers.setMimeType("application/json; charset=utf-8")

  let fromJson<'a> json =
    JsonConvert.DeserializeObject(json, typeof<'a>) :?> 'a

  let getResourceFromRequest<'a> (req : HttpRequest) =
    let getString rawForm =
        System.Text.Encoding.UTF8.GetString(rawForm)
    req.rawForm |> getString |> fromJson<'a>


  let rest resourceName resource =
    let resourcePath = "/" + resourceName
    let resourceIdPath = new PrintfFormat<(string -> string),string,string,string,string>(resourcePath + "/%s")

    let badRequest = BAD_REQUEST "Resource not found"

    let getAll = warbler (fun _ -> resource.GetAll () |> JSON)
    
    let handleResource requestError = function
    | Some r -> r |> JSON
    | _ -> requestError

    let getResourceById =
        resource.GetById >> handleResource (NOT_FOUND "Resource not found")
      
    path resourcePath >=> choose [
        GET >=> getAll
        GET >=> request (getResourceFromRequest >> resource.GetById >> handleResource badRequest)
        POST >=> request (getResourceFromRequest >> resource.Create >> JSON)
     ]


