import { initializeAiSimulator } from './fable/UI.Simulator';

let basicAiJs = function(cells) {
    let maxResourcesDiff = 0;
    let selectedTuple;
    cells.forEach(function(c) {
        c.Neighbours.forEach(function(n) {
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


let createJsObjectFromFields = function () {
    let codeEditor = ace.edit("code");
    let ai = new Object();
    ai.AiName = document.getElementById('aiName').value;
    ai.UserId = document.getElementById('userId').value;
    ai.Password = document.getElementById('password').value;
    ai.Content = codeEditor.getValue();
    return ai;
}

let submit = function () {
    let url = "http://localhost:8080/ais";

    let xhr = new XMLHttpRequest();
    xhr.open("POST", url, true);
    xhr.setRequestHeader("Content-type", "application/json");
    xhr.onreadystatechange = function() {
        if (xhr.readyState == XMLHttpRequest.DONE) {
            if (xhr.status == 200) {
                alert("AI saved");
            } else {
                alert('An error occured while saving AI ' + xhr.status.toString());
            }
        }
    }

    let data = JSON.stringify(createJsObjectFromFields());
    xhr.send(data);
}

let submitButton = document.getElementById("submit");
submitButton.addEventListener("click", submit);