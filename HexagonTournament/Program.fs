open HexagonTournament.Condorcet

[<EntryPoint>]
let main argv = 
    printfn "%A" argv
    let tournamentName = argv.[1]
    if argv.[0] = "drawGames" then
        let availableAis = HexagonTournament.SqlStorage.getAllAis () |> Seq.toList
        let nbBasicAisToRemove = (availableAis |> Seq.length) % 6
        let ais = 
            availableAis 
            //|> Seq.filter (fun x -> x.UserId = "hyj" && x.Password = "MiXiT" && x.AiName.Replace("Basic AI", ""))
        let games = drawGames (ais |> Seq.toList)
        ()
    else if argv.[0] = "determineBestPlayers" then
        ()
    0 // return an integer exit code
