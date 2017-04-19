module Hexagon.UI.Legend

open Hexagon.Domain
open Fable.Import.Browser

let mutable container: Element = null

let initialize domContainer =
    container <- domContainer

type Score = {
    CellsNb: int
    CellsNbContainer: HTMLDivElement
    ResourcesNb: int
    ResourcesNbContainer: HTMLDivElement
    BugsNb: int
    BugsNbContainer: HTMLDivElement
    Container: HTMLDivElement
}

let private scoreByAi = new System.Collections.Generic.Dictionary<int, Score>()

let rec reorderPlayersScore () =
    scoreByAi.Values 
    |> Seq.sortByDescending (fun v -> v.CellsNb, v.ResourcesNb, v.BugsNb) 
    |> Seq.iteri (fun i v -> v.Container.style.order <- string i)

let private onTerritoryChanged (territoryChanged:TerritoryChanged) =
    let previousScore = scoreByAi.[territoryChanged.AiId]
    let score = { previousScore with 
                    ResourcesNb = previousScore.ResourcesNb + territoryChanged.ResourcesIncrement; 
                    CellsNb = previousScore.CellsNb + territoryChanged.CellsIncrement }
    score.CellsNbContainer.textContent <- string score.CellsNb
    score.ResourcesNbContainer.textContent <- string score.ResourcesNb
    scoreByAi.[territoryChanged.AiId] <- score

    reorderPlayersScore ()

let private onBugged aiId =
    let previousScore = scoreByAi.[aiId]
    let score = { previousScore with BugsNb = previousScore.BugsNb + 1 }
    scoreByAi.[aiId] <- score
    score.BugsNbContainer.textContent <- string score.BugsNb

let private addAiInLegend (ai: AiDescription) =
    let score = document.createElement_div();
    score.className <- "score";
    score.setAttribute("aiId", string ai.Id);
    container.appendChild(score) |> ignore;
        
    let legendContainer = document.createElement_div();
    legendContainer.className <- "score-legend";
    score.appendChild(legendContainer) |> ignore;
        
    let nameContainer = document.createElement_div();
    nameContainer.className <- "score-aiName";
    nameContainer.textContent <- ai.Name;
    score.appendChild(nameContainer) |> ignore;
        
    let cellsNbContainer = document.createElement_div();
    cellsNbContainer.className <- "score-cellsNb";
    let cellsNb = 0;
    cellsNbContainer.textContent <- string cellsNb;
    score.appendChild(cellsNbContainer) |> ignore;
        
    let resourcesNbContainer = document.createElement_div();
    resourcesNbContainer.className <- "score-resources";
    let resourcesNb = 0;
    resourcesNbContainer.textContent <- string resourcesNb;
    score.appendChild(resourcesNbContainer) |> ignore;

    let bugsNbContainer = document.createElement_div();
    bugsNbContainer.className <- "score-bugs";
    let bugsNb = 0;
    bugsNbContainer.textContent <- string bugsNb;
    score.appendChild(bugsNbContainer) |> ignore
        
    scoreByAi.Add(ai.Id, {
        CellsNb = cellsNb
        CellsNbContainer = cellsNbContainer
        ResourcesNb = resourcesNb
        ResourcesNbContainer = resourcesNbContainer
        BugsNb = bugsNb
        BugsNbContainer = bugsNbContainer
        Container = score
    })
        
let private clear () =
    [1..int container.childNodes.length] |> Seq.iter (fun _ -> container.firstChild |> container.removeChild |> ignore)
    
let private onScoreChanged = function
    | TerritoryChanged territoryChanged -> onTerritoryChanged territoryChanged
    | Bugged bugged -> onBugged bugged

let private updateScores events = events |> Seq.iter onScoreChanged

let apply = function
    | Started _ -> 
        clear ()

    | Board (AiAdded aiDescription, _, scoreChanged) ->
        addAiInLegend aiDescription
        updateScores scoreChanged

    | Board (_, _, scoreChanged) ->
        updateScores scoreChanged

    | _ -> ()