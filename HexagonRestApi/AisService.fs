module HexagonRestApi.AisService

open HexagonRestApi.Domain.Domain
open BCrypt.Net

let aiToAiWithEncrytedPwd ai =    
    let hashedPwd = BCrypt.HashPassword(ai.Password, "$2a$12$VoOS3a0HBmjN67IoRtXV9e");
    {AiName=ai.AiName;UserId=ai.UserId;Password=hashedPwd;Content=ai.Content}

let private buildAiId (ai: Ai)=
    String.concat "." [ai.UserId; ai.Password; ai.AiName;]

let getAis fromStorage =
    fromStorage.GetAll

let submitAi intoStorage ai =
    let aiWithEncryptedPwd = aiToAiWithEncrytedPwd ai
    let id = buildAiId aiWithEncryptedPwd
    if intoStorage.Exists(id) then
        intoStorage.Update(id,aiWithEncryptedPwd)
    else
        intoStorage.Add(id,aiWithEncryptedPwd)        

let getAi fromStorage ai=
    let aiWithEncryptedPwd = aiToAiWithEncrytedPwd ai
    let id = buildAiId aiWithEncryptedPwd
    if fromStorage.Exists id then
        Some (fromStorage.GetById id)
    else
        None