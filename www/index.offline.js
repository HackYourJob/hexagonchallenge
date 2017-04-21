import { initializeAiSimulator } from '../build/UI.Simulator';

let basicAiJs = function (cells) {
    let maxResourcesDiff = 0;
    let selectedTuple;
    cells.forEach(function (c) {
        c.Neighbours.forEach(function (n) {
            let resourcesDiff = c.Resources - n.Resources;
            if (n.Owner.Case !== "Own" && resourcesDiff > maxResourcesDiff) {
                maxResourcesDiff = resourcesDiff;
                selectedTuple = [c, n];
            }
        });
    });
    return { FromId: selectedTuple[0].Id, ToId: selectedTuple[1].Id, AmountToTransfer: selectedTuple[0].Resources };
}

initializeAiSimulator(basicAiJs);


let codeEditor = ace.edit("code");
let aisUrl = "/ais";

let aiName = function () {
    return document.getElementById('aiName').value;
}
let userId = function() {
    return document.getElementById('userId').value;
}
let password = function() {
    return document.getElementById('password').value;
}

let createJsObjectFromFields = function () {
    let ai = new Object();
    ai.AiName = aiName();
    ai.UserId = userId();
    ai.Password = password();
    ai.Content = codeEditor.getValue();
    return ai;
}

let ajax = function(method, url, data, onResult, onfailed) {
    const xhr = new XMLHttpRequest();
    xhr.open(method, url, true);
    xhr.setRequestHeader("Content-type", "application/json");
    xhr.onreadystatechange = function () {
        if (xhr.readyState === XMLHttpRequest.DONE) {
            if (xhr.status === 200) {
                onResult(xhr.responseText);
            } else {
                onfailed(xhr.status.toString());
            }
        }
    }

    xhr.send(data ? JSON.stringify(data) : data);
};

let submit = function () {
    ajax("POST",
        aisUrl,
        createJsObjectFromFields(),
        function() { alert("AI saved"); },
        function(status) { alert(`An error occured while saving AI ${status}`); });
}

let submitButton = document.getElementById("submit");
submitButton.addEventListener("click", submit);

let getAi = function () {
    ajax("POST",
        aisUrl + "/get",
        createJsObjectFromFields(),
        function (result) { codeEditor.setValue(result); },
        function (status) { alert(`An error occured while retrieving AI ${status}`); });
}

let getButton = document.getElementById("get");
getButton.addEventListener("click", getAi);