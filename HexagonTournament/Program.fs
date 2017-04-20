open HexagonTournament.Condorcet

[<EntryPoint>]
let main argv = 
    printfn "%A" argv
    let tournamentName = argv.[1]
    if argv.[0] = "drawGames" then
        let availableAis = HexagonTournament.SqlStorage.getAllAis ()
        let nbBasicAisToAdd = (6 - ((availableAis |> Seq.length) % 6)) % 6
        let basicAisToAdd = HexagonTournament.SqlStorage.getBasicAis () |> List.take nbBasicAisToAdd
        let ais = availableAis @ basicAisToAdd
        let games = drawGames ais
        HexagonTournament.SqlStorage.queueGames games tournamentName
    else if argv.[0] = "determineBestPlayers" then
        let gamesResults = 
            HexagonTournament.SqlStorage.getGamesResults tournamentName
            |> Seq.groupBy (fun x -> x.MatchId)
            |> Seq.map (fun (game, results) -> 
                results
                |> Seq.sortBy (fun x -> x.Order)
                |> Seq.map (fun x -> 
                    {
                        AiId = x.AiId
                        AiName = x.AiName
                        Resources = x.Resources
                        Cells = x.Cells
                        Bugs = x.Bugs
                    })
                |> Seq.toList)
        let tournamentBestPlayers = determineBestPlayers gamesResults
        tournamentBestPlayers
        |> Seq.iteri (fun i p -> printfn "===== %i =====\n%A" i p)
        ()
    0 // return an integer exit code
