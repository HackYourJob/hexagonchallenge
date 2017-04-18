namespace HexagonRestApi.AisService

module AisService =
  open HexagonRestApi.Domain.Domain
 
  let private buildAiId userId password aiName =
    String.concat "." [userId; password; aiName;]
  
  let getAis fromStorage =
    fromStorage.GetAll

  let submitAi intoStorage ai =
    let id = buildAiId ai.UserId ai.Password ai.AiName
    if intoStorage.Exists(id) then
        intoStorage.Update(id,ai)
    else
        intoStorage.Add(id,ai)        

  let getAi fromStorage aiInfo=
    let userId, password, aiName = aiInfo
    let id = buildAiId userId password aiName
    if fromStorage.Exists id then
      Some (fromStorage.GetById id)
    else
      None