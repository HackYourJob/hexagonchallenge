import { startGame } from './fable/Game';

var cellDomsByPosition = [];

var saveCell = function (rowPosition, columnPosition, cell) {
    var currentAiId = -1;
    var currentResource = -1;
    var isInside = false;
    var contentDom = cell.querySelector('.board-cell-content');

    cellDomsByPosition[rowPosition + "-" + columnPosition] = {
        setAiId: function (num) {
            if (num !== currentAiId) {
                cell.setAttribute('aiId', num);
                currentAiId = num;
            }
        },
        setResources: function (nb) {
            if (currentResource !== nb) {
                contentDom.textContent = nb;
                currentResource = nb;
            }
        },
        inside: function (value) {
            if (isInside !== value) {
                cell.setAttribute('inside', value);
                isInside = value;
            }
        }
    };
};

var getCell = function (rowPosition, columnPosition) {
    return cellDomsByPosition[rowPosition + "-" + columnPosition];
};

var createCell = function (rowPosition, columnPosition) {
    var cell = document.createElement('div');
    cell.className = 'board-cell hexagonCellShape';

    cell.setAttribute('data-column', columnPosition);
    cell.setAttribute('data-row', rowPosition);

    cell.innerHTML = '<div class="hexagonCellShape-left"></div><div class="hexagonCellShape-middle"></div><div class="hexagonCellShape-right"></div><div class="board-cell-content board-cell-content--hexagon">0</div>';

    return cell;
};

var createAndSaveCell = function (rowPosition, columnPosition) {
    var cell = createCell(rowPosition, columnPosition);

    saveCell(rowPosition, columnPosition, cell);

    return cell;
};

var createRow = function (rowPosition, columnsNb) {
    var row = document.createElement("div");
    row.className = "board-row board-row--hexagon";
    for (var columnNum = 1; columnNum <= columnsNb; columnNum++) {
        row.appendChild(createAndSaveCell(rowPosition, columnNum));
    }

    return row;
};

var initializeBoard = function (rowsNb, columnsNb) {
    var board = document.querySelector("#board");

    for (var rowNum = 1; rowNum <= rowsNb + 1; rowNum++) {
        board.appendChild(createRow(rowNum, columnsNb));
    }
};

var initializeCells = function (cells) {
    cells.forEach(function (cell) {
        var boardCell = getCell(cell["Id"]["LineNum"], cell["Id"]["ColumnNum"]);

        boardCell.inside(true);

        if (cell["State"].hasOwnProperty("Free")) {
            boardCell.setAiId(0);
            boardCell.setResources(cell["State"]["Free"]);
        } else {
            var own = cell["State"]["Own"];
            boardCell.setAiId(own.AiId);
            boardCell.setResources(own.Resources);
        }
    });
};

var scoresByAi = [];

var initializeLegend = function (ais) {
    var scores = document.querySelector("#scores");
    ais.forEach(function (ai) {
        var score = document.createElement("div");
        score.className = "score";
        score.setAttribute('aiId', ai.Id);
        scores.appendChild(score);
        var legendContainer = document.createElement("div");
        legendContainer.className = "score-legend";
        score.appendChild(legendContainer);
        var nameContainer = document.createElement("div");
        nameContainer.className = "score-aiName";
        nameContainer.textContent = ai.Name;
        score.appendChild(nameContainer);
        var cellsNbContainer = document.createElement("div");
        cellsNbContainer.className = "score-cellsNb";
        var cellsNb = 0;
        cellsNbContainer.textContent = cellsNb;
        score.appendChild(cellsNbContainer);
        var resourcesNbContainer = document.createElement("div");
        resourcesNbContainer.className = "score-resources";
        var resourcesNb = 0;
        resourcesNbContainer.textContent = resourcesNb;
        score.appendChild(resourcesNbContainer);
        var bugsNbContainer = document.createElement("div");
        bugsNbContainer.className = "score-bugs";
        var bugsNb = 0;
        bugsNbContainer.textContent = bugsNb;
        score.appendChild(bugsNbContainer);

        scoresByAi[ai.Id] = {
            incrementResources: function (nb) {
                resourcesNb += nb;
                resourcesNbContainer.textContent = resourcesNb;
            },
            incrementCells: function (nb) {
                cellsNb += nb;
                cellsNbContainer.textContent = cellsNb;
            },
            incrementBugs: function (nb) {
                bugsNb += nb;
                bugsNbContainer.textContent = bugsNb;
            }
        };


    });
};

var onStarted = function (evt) {
    initializeBoard(evt["BoardSize"]["Lines"], evt["BoardSize"]["Columns"]);
    initializeCells(evt["Board"]);
    initializeLegend(evt["Ais"]);
};

var onCellOwned = function (evt) {
    var cellId = evt.CellId;
    var boardCell = getCell(cellId["LineNum"], cellId["ColumnNum"]);
    boardCell.setAiId(evt.AiId);
    boardCell.setResources(evt.Resources);
};

var onCellResourcesChanged = function (evt) {
    var cellId = evt.CellId;
    var boardCell = getCell(cellId["LineNum"], cellId["ColumnNum"]);
    boardCell.setResources(evt.Resources);
};

var onCellsChanged = function (events) {
    events.forEach(function (evt) {
        if (evt["Owned"]) {
            onCellOwned(evt["Owned"]);
            return;
        }
        if (evt["ResourcesChanged"]) {
            onCellResourcesChanged(evt["ResourcesChanged"]);
            return;
        }
    });
};

var onBugged = function (evt) {
    scoresByAi[evt].incrementBugs(1);
};

var onTerritoryChanged = function (evt) {
    scoresByAi[evt.AiId].incrementCells(evt.CellsIncrement);
    scoresByAi[evt.AiId].incrementResources(evt.ResourcesIncrement);
};

var onScoreChanged = function (events) {
    events.forEach(function (evt) {
        if (evt["TerritoryChanged"]) {
            onTerritoryChanged(evt["TerritoryChanged"]);
            return;
        }
        if (evt["Bugged"]) {
            onBugged(evt["Bugged"]);
            return;
        }
    });
};

var handleMessage = function (message) {
    if (message["Started"]) {
        onStarted(message["Started"]);
        return;
    }
    if (message["Board"]) {
        onCellsChanged(message["Board"][1]);
        onScoreChanged(message["Board"][2]);
        return;
    }
};

startGame(handleMessage, null, 5);

/*var ws = new WebSocket("ws://localhost:8080/websocket");
ws.onerror = function (ev) {
    console.log('error');
    console.log(ev);
};
ws.onabort = function () {
    console.log('abort');
};
ws.onopen = function () {
    console.log("Opened");

    ws.send('{ "Echo": "Hello" }');
};
ws.onclose = function () {
    console.log("Closed");
};
ws.onmessage = function (evt) {
    JSON.parse(evt.data).forEach(handleMessage);
};*/