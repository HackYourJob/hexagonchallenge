module Hexagon.UI

open Hexagon.Domain
open Fable.Import.Browser

type CellDom = {
    AiId: int
    Resources: int
    IsInside: bool
    Cell: HTMLDivElement
}

let cellsDomByPosition = new System.Collections.Generic.Dictionary<string, CellDom>()

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

let updateResources resources cellId cell =
    changeOwner cell.AiId resources cellId cell

let initializeCells cells =
    cells
    |> Seq.iter (fun c ->
        let cell = getCell c.Id.LineNum c.Id.ColumnNum
        cell |> inside true c.Id
        match c.State with
        | Free n -> cell |> changeOwner 0 n c.Id
        | Own own -> cell |> changeOwner own.AiId own.Resources c.Id)

let onStarted started =
    initializeBoard started.BoardSize
    initializeCells started.Board
    //initializeLegend started.Ais

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

let onScoreChanged scoreChanged =
    ()

let handleMessage = function
    | Started started -> onStarted started
    | Board (boardEvents, cellsChanged, scoreChanged) ->
        onCellsChanged cellsChanged
        onScoreChanged scoreChanged
    | _ -> ()
