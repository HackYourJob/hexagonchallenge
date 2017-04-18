namespace HexagonRestApi.AisStorage

type Ai = {
  AiName : string
  UserId : string
  Password : string
  Content : string
}

module AisStorage =
  open System.Collections.Generic

  let private aiStorage = new Dictionary<string, Ai>()
  
  let buildAiId userId password aiName =
    String.concat "." [userId; password; aiName;]
  
  let getAis () =
    aiStorage.Values |> Seq.map (fun ai -> ai)

  let submitAi ai =
    let id = buildAiId ai.UserId ai.Password ai.AiName
    aiStorage.Add(id, ai)
    ai

  let getAi aiInfo =
    let userId, password, aiName = aiInfo
    let id = buildAiId userId password aiName
    if aiStorage.ContainsKey(id) then
      Some aiStorage.[id]
    else
      None