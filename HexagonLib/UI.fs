module Hexagon.UI

open Hexagon.Domain
open Fable.Import.Browser

type CellDom = {
    AiId: int
    Resources: int
    IsInside: bool
    Cell: HTMLDivElement
}

type Score = {
    CellsNb: int
    CellsNbContainer: HTMLDivElement
    ResourcesNb: int
    ResourcesNbContainer: HTMLDivElement
    BugsNb: int
    BugsNbContainer: HTMLDivElement
}

let cellsDomByPosition = new System.Collections.Generic.Dictionary<string, CellDom>()
let scoreByAi = new System.Collections.Generic.Dictionary<int, Score>()

let createCell row col =
    let cell = document.createElement_div();
    cell.className <- "board-cell hexagonCellShape";

    cell.setAttribute("data-column", string col);
    cell.setAttribute("data-row", string row);

    cell.innerHTML <- "<div class='hexagonCellShape-left'></div><div class='hexagonCellShape-middle'></div><div class='hexagonCellShape-right'></div><div class='board-cell-content board-cell-content--hexagon'>0</div>";
    cell

let saveCell row col cell =
    cellsDomByPosition.[sprintf "%i-%i" row col] <- {
        AiId = -1
        Resources = -1
        IsInside = false
        Cell = cell
    }

let getCell row col =
    cellsDomByPosition.[sprintf "%i-%i" row col]

let createAndSaveCell row col =
    let cell = createCell row col
    saveCell row col cell
    cell

let createRow row columnsNb =
    let rowElement = document.createElement_div()
    rowElement.className <- "board-row board-row--hexagon"
    [1..columnsNb]
    |> Seq.iter (fun col -> createAndSaveCell row col |> rowElement.appendChild |> ignore)
    rowElement

let initializeBoard boardSize =
    let board = document.querySelector("#board")
    [1..int board.childNodes.length] |> List.map (fun n -> board.removeChild <| board.childNodes.item(0.)) |> ignore
    [1..boardSize.Lines + 1]
    |> Seq.iter (fun row -> createRow row <| boardSize.Columns |> board.appendChild |> ignore)

let inside value cellId cell =
    cell.Cell.setAttribute("inside", string value)
    cellsDomByPosition.[sprintf "%i-%i" cellId.LineNum cellId.ColumnNum] <- 
        { cell with IsInside = value }

let changeOwner aiId resources cellId cell =
    cell.Cell.setAttribute("aiId", string aiId)
    cell.Cell.querySelector(".board-cell-content").textContent <- string resources
    cellsDomByPosition.[sprintf "%i-%i" cellId.LineNum cellId.ColumnNum] <- 
        { cell with AiId = aiId; Resources = resources }

let updateResources resources cellId (cell:CellDom) =
    changeOwner cell.AiId resources cellId cell

let initializeCells cells =
    cells
    |> Seq.iter (fun c ->
        let cell = getCell c.Id.LineNum c.Id.ColumnNum
        cell |> inside true c.Id
        match c.State with
        | Free n -> cell |> changeOwner 0 n c.Id
        | Own own -> cell |> changeOwner own.AiId own.Resources c.Id)

let addAiInLegend (ai: AiDescription) =
    let scores = document.querySelector("#scores");

    let score = document.createElement_div();
    score.className <- "score";
    score.setAttribute("aiId", string ai.Id);
    scores.appendChild(score) |> ignore;
        
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
        
    scoreByAi.[ai.Id] <- {
        CellsNb = cellsNb
        CellsNbContainer = cellsNbContainer
        ResourcesNb = resourcesNb
        ResourcesNbContainer = resourcesNbContainer
        BugsNb = bugsNb
        BugsNbContainer = bugsNbContainer
    }

let initializeLegend () =
    let scores = document.querySelector("#scores");
    [1..int scores.childNodes.length - 1] |> List.map (fun _ -> scores.removeChild <| scores.childNodes.item(1.0)) |> ignore

let onStarted started =
    initializeBoard started.BoardSize
    initializeCells started.Board
    initializeLegend ()

let onCellOwned (cellOwned:CellOwned) =
    getCell cellOwned.CellId.LineNum cellOwned.CellId.ColumnNum
    |> changeOwner cellOwned.AiId cellOwned.Resources cellOwned.CellId

let onCellResourcesChanged resourcesChanged =
    getCell resourcesChanged.CellId.LineNum resourcesChanged.CellId.ColumnNum
    |> updateResources resourcesChanged.Resources resourcesChanged.CellId

let onCellsChanged cellsChanged =
    cellsChanged
    |> Seq.iter (fun e -> 
        match e with
        | Owned cellOwned -> onCellOwned cellOwned
        | ResourcesChanged resourcesChanged -> onCellResourcesChanged resourcesChanged)

let moveUp score upperElement = 
    let scores = score.CellsNbContainer.parentElement.parentElement
    scores.removeChild score.CellsNbContainer.parentElement |> ignore
    scores.insertBefore (score.CellsNbContainer.parentElement, upperElement) |> ignore

let moveDown score (lowerElement:Node) = 
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

let onTerritoryChanged (territoryChanged:TerritoryChanged) =
    let previousScore = scoreByAi.[territoryChanged.AiId]
    let score = { previousScore with 
                    ResourcesNb = previousScore.ResourcesNb + territoryChanged.ResourcesIncrement; 
                    CellsNb = previousScore.CellsNb + territoryChanged.CellsIncrement }
    score.CellsNbContainer.textContent <- string score.CellsNb
    score.ResourcesNbContainer.textContent <- string score.ResourcesNb
    scoreByAi.[territoryChanged.AiId] <- score
    reorderPlayersScore score territoryChanged.CellsIncrement

let onBugged aiId =
    let previousScore = scoreByAi.[aiId]
    let score = { previousScore with BugsNb = previousScore.BugsNb + 1 }
    scoreByAi.[aiId] <- score
    score.BugsNbContainer.textContent <- string score.BugsNb

let onScoreChanged scoreChanged =
    scoreChanged
    |> Seq.iter (fun e -> 
        match e with
        | TerritoryChanged territoryChanged -> onTerritoryChanged territoryChanged
        | Bugged bugged -> onBugged bugged)

let handleMessage = function
    | Started started -> onStarted started
    | Board (AiAdded aiDescription, cellsChanged, scoreChanged) ->
        addAiInLegend aiDescription
        onCellsChanged cellsChanged
        onScoreChanged scoreChanged
    | Board (boardEvents, cellsChanged, scoreChanged) ->
        onCellsChanged cellsChanged
        onScoreChanged scoreChanged
    | _ -> ()

let editor = Fable.Import.Globals.ace.edit(document.getElementById("code"))
editor.getSession().setMode("ace/mode/javascript");
editor.setValue("""(function() {
    return function(cells) {
        // cells if an array of cell you own { Id : 'some-id', Resources : 12, Neighbours : [...] }, where 
        // - Resources cannot be more than 100, it is the available resources to move from a cell
        // - Neighbours is an array of cell, with Id and Resources properties only
        // NOTE: you don't know who is the cell's owner, and by the way, Neighbours include your cells also
    
        // Your clever code here :)
    
        if (cells[cells.length - 1].Resources > 10 && cells[cells.length - 1].Resources > cells[0].Neighbours[0].Resources) {
            return { FromId: cells[cells.length - 1].Id, ToId: cells[0].Neighbours[0].Id, AmountToTransfer: cells[cells.length - 1].Resources - 1 };
        }
    };
})""") |> ignore;

open Fable.Core
open Hexagon.BasicAi

[<Emit("eval($0)()")>]
let compile (code: string): (Ais.AiCell[] -> Ais.TransactionParameters option) = jsNative

let getPlayFunction (): (Ais.AiCell[] -> Ais.TransactionParameters option) =
    editor.getValue() |> compile

[<Emit("setTimeout($1, $0)")>]
let setTimeout (delayInMs: int) (action: unit -> unit) = jsNative

let startGame hexagonSize ais roundsNb isCancelled = 
    let rec deferNextStep nextStep = 
        match isCancelled(), nextStep with
        | true, _ -> ()
        | false, NextRound (num, action) -> 
                setTimeout 10 (fun () -> action() |> deferNextStep)
        | false, End (reason, score) -> ()

    let nextStep = Hexagon.Game.startGame handleMessage hexagonSize roundsNb ais

    deferNextStep nextStep