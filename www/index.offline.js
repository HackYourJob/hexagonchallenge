import { initializeAiSimulator } from './fable/UI.Simulator';

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
let url = "http://localhost:8080/ais";

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

//let submit = function () {
//    let xhr = new XMLHttpRequest();
//    xhr.open("POST", url, true);
//    xhr.setRequestHeader("Content-type", "application/json");
//    xhr.onreadystatechange = function () {
//        if (xhr.readyState === XMLHttpRequest.DONE) {
//            if (xhr.status === 200) {
//                alert("AI saved");
//            } else {
//                alert(`An error occured while saving AI ${xhr.status.toString()}`);
//            }
//        }
//    }

//    let data = JSON.stringify(createJsObjectFromFields());
//    xhr.send(data);
//}

//let submitButton = document.getElementById("submit");
//submitButton.addEventListener("click", submit);

let getAi = function () {
    var xhr = new XMLHttpRequest();
    xhr.onreadystatechange = function () {
        if (xhr.readyState === XMLHttpRequest.DONE) {
            if (xhr.status === 200) {
                let aiJson = JSON.parse(xhr.responseText);
                codeEditor.setValue(aiJson.content);
            } else {
                alert(`An error occured while retrieving AI ${xhr.status.toString()}`);
            }
        }
    };
    let getUrlById = url + "/userId=" + userId() + "&password=" + password() + "&aiName=" + aiName();
    xhr.open("GET", getUrlById, true);
    xhr.send();
}

let getButton = document.getElementById("get");
getButton.addEventListener("click", getAi);