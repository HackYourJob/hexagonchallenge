module AiStorageInMySql

open Dapper
open MySql.Data
open MySql.Data.MySqlClient
open HexagonRestApi.Domain
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
