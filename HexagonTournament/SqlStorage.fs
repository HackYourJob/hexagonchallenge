module HexagonTournament.SqlStorage

open Dapper
open MySql.Data
open MySql.Data.MySqlClient
open HexagonTournament.Condorcet

let connectionString = System.Environment.GetEnvironmentVariable("HEXAGON_MYSQL_CONNECTIONSTRING")

type GameAi = {
    GameId: string
    AiId: string
    Order: int
}

type GameQueue = {
    GameId: string
    CreatedAt: System.DateTime
    TournamentName: string
}

type Tournament = {
    TournamentName: string
}

type GameResult = {
    MatchId: System.Guid
    AiId: string
    AiName: string
    Resources: int
    Cells: int
    Bugs: int
    Order: int
}

let getAllAis () =
    use connection = new MySqlConnection(connectionString)
    connection.Query<string>("SELECT AiId FROM ai  WHERE userId <> 'hyj' AND aiName NOT LIKE 'Basic AI%' AND password <> '$2a$12$VoOS3a0HBmjN67IoRtXV9ebip4qkKoj86NwUEChEJ.LwlMcb9nQ66'") |> Seq.toList

let getBasicAis () =
    use connection = new MySqlConnection(connectionString)
    connection.Query<string>("SELECT AiId FROM ai WHERE userId = 'hyj' AND aiName LIKE 'Basic AI%' AND password = '$2a$12$VoOS3a0HBmjN67IoRtXV9ebip4qkKoj86NwUEChEJ.LwlMcb9nQ66' LIMIT 6") |> Seq.toList

let queueGames games tournamentName =
    let now = System.DateTime.Now
    use connection = new MySqlConnection(connectionString)
    connection.Open()
    games
    |> Seq.iter (fun g -> 
        let game = { GameId = System.Guid.NewGuid().ToString(); CreatedAt = now; TournamentName = tournamentName }
        g 
        |> Seq.iteri (fun i p ->
            let ai = { GameId = game.GameId; AiId = p; Order = i + 1 }
            connection.Execute("INSERT INTO matchAi VALUES (@GameId, @AiId, @Order)", ai) |> ignore)
        connection.Execute("INSERT INTO matchQueue VALUES (@GameId, NULL, @CreatedAt, @TournamentName)", game) |> ignore)
    connection.Close()

let getGamesResults tournamentName =
    use connection = new MySqlConnection(connectionString)
    connection.Query<GameResult>(
        "SELECT matchResult.MatchId, ai.AiId, ai.AiName, matchResult.Resources, 
            matchResult.Cells, matchResult.Bugs, matchResult.Order
         FROM matchResult, ai, matchQueue
         WHERE matchResult.AiId = ai.AiId
         AND matchResult.MatchId = matchQueue.MatchId
         AND matchQueue.TournamentName = @TournamentName", { TournamentName = tournamentName }) |> Seq.toList
