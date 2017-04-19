module HexagonTournament.SqlStorage

open Dapper
open MySql.Data
open MySql.Data.MySqlClient
open HexagonRestApi.Domain

let connectionString = System.Environment.GetEnvironmentVariable("HEXAGON_MYSQL_CONNECTIONSTRING")

let getAllAis () =
    use connection = new MySqlConnection(connectionString)
    connection.Query<string>("SELECT AiId FROM ai") |> seq
