namespace HexagonRestApi.AisService

module AisService =
  open HexagonRestApi.Domain.Domain
 
   let private buildAiId (ai: Ai)=
    String.concat "." [ai.UserId; ai.Password; ai.AiName;]
 
  let getAis fromStorage =
    fromStorage.GetAll

  let submitAi intoStorage ai =
    let id = buildAiId ai
    if intoStorage.Exists(id) then
        intoStorage.Update(id,ai)
    else
        intoStorage.Add(id,ai)        

  let getAi fromStorage ai=
    let id = buildAiId ai
    if fromStorage.Exists id then
      Some (fromStorage.GetById id)
    else
      None