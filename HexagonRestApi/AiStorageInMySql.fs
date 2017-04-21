module AiStorageInMySql

open HexagonRestApi.Domain
open Dapper
open MySql.Data
open MySql.Data.MySqlClient
open HexagonRestApi
open System.Security.Cryptography

let connectionString = System.Environment.GetEnvironmentVariable("HEXAGON_MYSQL_CONNECTIONSTRING")

type Id = {
    Id: string
}

type AiInDb = {
    AiId: string
    AiName : string
    UserId : string
    Password : string
    Content : string
}

let private getHashedId (id: string) =
    use sha256 = new SHA256Managed()
    let bytes = System.Text.Encoding.UTF8.GetBytes(id)
    let hashedBytes = sha256.ComputeHash(bytes)
    let buffer = new System.Text.StringBuilder()
    (hashedBytes
    |> Seq.fold (fun (sb: System.Text.StringBuilder) b -> sb.AppendFormat("{0:x2}", b)) buffer)
        .ToString()

let getAll () =
    use connection = new MySqlConnection(connectionString)
    connection.Query<Domain.Ai>("SELECT aiName, userId, password, content FROM ai") |> seq

let exists id =
    use connection = new MySqlConnection(connectionString)
    connection.ExecuteScalar<bool>("SELECT count(*) FROM ai WHERE aiId = @id", { Id = getHashedId id })

let add (id, (ai:Domain.Ai)) =
    use connection = new MySqlConnection(connectionString)
    connection.Execute(
        "INSERT INTO ai VALUES (@AiId, @UserId, @AiName, @Password, @Content)", 
        { AiId = getHashedId id; AiName = ai.AiName; UserId = ai.UserId; Password = ai.Password; Content = ai.Content } )
    |> ignore
    ai

let update (id, (ai:Domain.Ai)) =
    use connection = new MySqlConnection(connectionString)
    connection.Execute(
        "UPDATE ai 
        SET Content = @Content 
        WHERE AiId = @AiId", 
        { AiId = getHashedId id; AiName = ai.AiName; UserId = ai.UserId; Password = ai.Password; Content = ai.Content } )
    |> ignore
    ai

let getById id =
    use connection = new MySqlConnection(connectionString)
    connection.QuerySingle<Domain.Ai>("SELECT aiName, userId, password, content FROM ai WHERE AiId = @Id", { Id = getHashedId id })
    
let getByCode id =
    use connection = new MySqlConnection(connectionString)
    connection.QuerySingle<string>("SELECT content FROM ai WHERE AiId = @Id", { Id = getHashedId id })

let tryToGetCode id ai =
    match exists id with
    | false -> None
    | true -> 
        let code = getByCode id
        match System.String.IsNullOrWhiteSpace(code) with
        | true -> None
        | false -> Some code

let updateOrAdd id ai =
    match exists id with
    | false -> add (id, ai) |> ignore
    | true -> update (id, ai) |> ignore

let getMatchEvents (matchId: string) =
    use connection = new MySqlConnection(connectionString)
    
    connection.QuerySingle<byte[]>("SELECT events FROM matchEvents WHERE matchId = @Id", { Id = matchId })

let getTournamentNames () =
    use connection = new MySqlConnection(connectionString)
    connection.Query<string>("SELECT DISTINCT tournamentName FROM matchQueue WHERE lockedBy IS NOT NULL")
    |> Seq.toArray

type MatchResult = {
    Id: System.Guid
    Date: System.DateTime
    AiName: string
}

let getMatchsOfTournament tournamentId =
    use connection = new MySqlConnection(connectionString)
    connection.Query<MatchResult>("""
        SELECT matchQueue.matchId as Id, matchEvents.processedAt as Date, ai.ainame as AiName
        FROM matchQueue 
        LEFT JOIN matchEvents ON matchEvents.matchId = matchQueue.matchId 
        LEFT JOIN matchResult ON matchResult.matchId = matchQueue.matchId 
        LEFT JOIN ai ON matchResult.aiId = ai.aiId 
        WHERE matchEvents.events IS NOT NULL 
        AND tournamentName = @Id""", { Id = tournamentId })
    |> Seq.groupBy (fun m -> m.Id)
    |> Seq.map (fun (id, values) -> { Id = id.ToString(); Date = (values |> Seq.head).Date; AiNames = values |> Seq.map (fun v -> v.AiName) |> Seq.toArray} )
    |> Seq.toArray