namespace SuaveRestApi.Db
open System.Collections.Generic
type Ai = {
  AiName : string
  UserName : string
  Password : string
}
module Db =
  let private aiStorage = new Dictionary<string, Ai>()
  let getAis () =
    aiStorage.Values |> Seq.map (fun ai -> ai)

