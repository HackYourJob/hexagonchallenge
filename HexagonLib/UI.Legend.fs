module Hexagon.UI.Legend

open Hexagon.Domain
open Fable.Import.Browser

let mutable container: Element = null

let initialize () =
    container <- document.querySelector("#scores")

type Score = {
    CellsNb: int
    CellsNbContainer: HTMLDivElement
    ResourcesNb: int
    ResourcesNbContainer: HTMLDivElement
    BugsNb: int
    BugsNbContainer: HTMLDivElement
}

let private scoreByAi = new System.Collections.Generic.Dictionary<int, Score>()

let private moveUp score upperElement = 
    let scores = score.CellsNbContainer.parentElement.parentElement
    scores.removeChild score.CellsNbContainer.parentElement |> ignore
    scores.insertBefore (score.CellsNbContainer.parentElement, upperElement) |> ignore

let private moveDown score (lowerElement:Node) = 
    let scores = score.CellsNbContainer.parentElement.parentElement
    scores.removeChild score.CellsNbContainer.parentElement |> ignore
    if lowerElement.nextSibling <> null then
        scores.insertBefore (score.CellsNbContainer.parentElement, lowerElement.nextSibling) |> ignore
    else
        scores.appendChild score.CellsNbContainer.parentElement |> ignore

let rec reorderPlayersScore score cellsIncrement =
    let surroundingElement, compare, move =
        if cellsIncrement > 0 then
            score.CellsNbContainer.parentElement.previousElementSibling, (>), moveUp
        else
            score.CellsNbContainer.parentElement.nextElementSibling, (<), moveDown
    
    if surroundingElement <> null then
        let surroundingCellsNb = surroundingElement.getElementsByClassName("score-cellsNb").Item 0 
        if compare score.CellsNb (int surroundingCellsNb.textContent) then
            move score surroundingElement
            reorderPlayersScore score cellsIncrement

let private onTerritoryChanged (territoryChanged:TerritoryChanged) =
    let previousScore = scoreByAi.[territoryChanged.AiId]
    let score = { previousScore with 
                    ResourcesNb = previousScore.ResourcesNb + territoryChanged.ResourcesIncrement; 
                    CellsNb = previousScore.CellsNb + territoryChanged.CellsIncrement }
    score.CellsNbContainer.textContent <- string score.CellsNb
    score.ResourcesNbContainer.textContent <- string score.ResourcesNb
    scoreByAi.[territoryChanged.AiId] <- score
    reorderPlayersScore score territoryChanged.CellsIncrement

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
    })
        
let private clear () =
    [1..int container.childNodes.length - 1] |> List.map (fun _ -> container.removeChild <| container.childNodes.item(1.0)) |> ignore
    
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