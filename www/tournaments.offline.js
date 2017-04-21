import { initialize, stop } from '../build/UI.Tournament';

let events = "";

let getRaw = function(url, onResult) {
    const xhr = new XMLHttpRequest();
    xhr.open("GET", url, true);
    xhr.setRequestHeader("Content-type", "application/json");
    xhr.onreadystatechange = function () {
        if (xhr.readyState === XMLHttpRequest.DONE) {
            if (xhr.status === 200) {
                onResult(xhr.responseText);
            }
        }
    }

    xhr.send();
};

let getJson = function (url, onResult) {
    getRaw(url, function(r) { onResult(JSON.parse(r)) });
};

let updateTournaments = function() {
    getJson("/tournaments",
        function(results) {
            let selector = document.getElementById("tournaments");

            results.forEach(function(r) {
                let opt = document.createElement('option');
                opt.value = r;
                opt.innerHTML = r;
                selector.appendChild(opt);
            });
        });
};

let runMatch = function (matchId) {
    getRaw("/matchs/"+ matchId +"/events",
        function (results) {
            let runner = document.getElementById("runner");
            runner.style.visibility = 'visible';

            events = results;
        });
}

let displayMatchs = function (tournament) {
    getJson("/tournaments/"+ tournament +"/matchs",
        function (results) {
            let container = document.getElementById("matchs");

            results.sort(function (a, b) {
                return new Date(a.date) - new Date(b.date);
            }).forEach(function (r) {
                let match = document.createElement('li');
                match.innerHTML =
                    '<span class="match-date">' +
                    moment(r.date).format('DD/MM/YY hh:mm:ss') +
                    ' (' +
                    moment(r.date).fromNow() +
                    ') : </span><span class="match-ai">' +
                    r.aiNames.join(", ") +
                    '</span>';
                let button = document.createElement('button');
                button.type = 'button';
                button.className = 'btn btn-success match-runButton';
                button.innerHTML = '<span class="glyphicon glyphicon-play"></span> Run';
                button.addEventListener("click", function() {
                    runMatch(r.id);
                });
                match.appendChild(button);
                container.appendChild(match);
            });
        });
}

let seeMatchsButton = document.getElementById("seeMatchs");
seeMatchsButton.addEventListener("click", function () {
    let selector = document.getElementById("tournaments");
    if (selector.selectedIndex >= 0) {
        displayMatchs(selector.options[selector.selectedIndex].value);
    }
});

let closeButton = document.getElementById("close");
closeButton.addEventListener("click", function () {
    let runner = document.getElementById("runner");
    runner.style.visibility = 'collapse';

    close();
});

updateTournaments();

var cleanEvents = function (events) {
    let flatCollection = function(obj) {
        if (obj && obj["$type"] === "Microsoft.FSharp.Collections.FSharpList") {
            return obj["$values"];
        }

        return obj;
    };

    let transformRec = function (obj) {
        if (!obj || typeof obj === 'string') return obj;

        for (let propertyName in obj) {
            obj[propertyName] = flatCollection(obj[propertyName]);

            obj[propertyName] = transformRec(obj[propertyName]);
        }

        if (Array.isArray(obj)) {
            for (let index in obj) {
                obj[index] = flatCollection(obj[index]);

                obj[index] = transformRec(obj[index]);
            }
        }

        return obj;
    };

    return transformRec(events);
};

initialize(() => events, cleanEvents);