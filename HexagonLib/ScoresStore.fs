module Hexagon.ScoresStore

open Domain

type Store ()=
    let scoresById = System.Collections.Generic.Dictionary<int, AiScore>()

    let updateScore aiId transform = 
        let score = 
            match scoresById.TryGetValue(aiId) with
            | true, value -> value
            | false, _ -> 
                let value = { CellsNb = 0; Resources = 0; BugsNb = 0 }
                scoresById.Add(aiId, value)
                value
                
        scoresById.[aiId] <- transform score

    member x.getScores () : Scores =
        scoresById |> Seq.map (fun v -> (v.Key, v.Value)) |> Seq.toList

    member x.tryToGetWinner () : AiId option =
        let aiIds = 
            scoresById
            |> Seq.filter (fun v -> v.Value.CellsNb > 0)
            |> Seq.map (fun v -> v.Key)
            |> Seq.toArray
        
        match aiIds with
        | [|id|] -> Some id
        | _ -> None

    member x.apply (evt: ScoreChanged) =
        match evt with
        | Bugged aiId -> 
                updateScore aiId (fun score -> { score with BugsNb = score.BugsNb + 1 })
        | TerritoryChanged { AiId = aiId; ResourcesIncrement = resourcesIncrement; CellsIncrement = cellsIncrement } ->
                updateScore aiId (fun score -> { score with Resources = score.Resources + resourcesIncrement; CellsNb = score.CellsNb + cellsIncrement })
