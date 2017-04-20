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
        ()
    0 // return an integer exit code
