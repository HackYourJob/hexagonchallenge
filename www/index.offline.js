import { startGame } from './fable/Game';
import { handleMessage, getPlayFunction } from './fable/UI';
import { TransactionParameters, Transaction } from "./fable/Ais";
import { play } from "./fable/BasicAi";

let ct = { isCancelled: false };
let stop = function () {
    ct.isCancelled = true;
}

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
    
let testButton = document.getElementById("test");
testButton.addEventListener("click", function() {
    const ais = [[{ Id: 1, Name: "Basic JS" }, basicAiJs ], 
        [{Id: 2, Name: "Basic F#" }, play ], 
        [{ Id: 3, Name: "Dynamic JS" }, getPlayFunction() ]];

    ct.isCancelled = false;
    setTimeout(stop, 30000);

    startGame(handleMessage, ct, 9, function (fun) {
        setTimeout(fun, 100);
    }, ais);    
});

let stopButton = document.getElementById("stop");
stopButton.addEventListener("click", stop);