module HexagonRestApi.AisService

open HexagonRestApi.Domain.Domain
open BCrypt.Net

let hashPassword password =
    BCrypt.HashPassword(password, "$2a$12$VoOS3a0HBmjN67IoRtXV9e")
    
let private buildAiId (ai: Ai)=
    String.concat "." [ai.UserId; ai.Password; ai.AiName;]

let submitAi updateOrAdd ai =
    updateOrAdd (ai |> buildAiId) { ai with Password = ai.Password |> hashPassword }

let getAi tryToGetId ai =
    tryToGetId (ai |> buildAiId) { ai with Password = ai.Password |> hashPassword }