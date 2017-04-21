namespace HexagonRestApi.Domain
type Ai = {
    AiName : string
    UserId : string
    Password : string
    Content : string
}

type Match = {
    Id: string
    Date: System.DateTime
    AiNames: string[]
}