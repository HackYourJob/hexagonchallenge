import { startGame } from './fable/Game';
import { handleMessage } from './fable/UI';
import { TransactionParameters, Transaction } from "./fable/Ais";
import { play } from "./fable/BasicAi";

let ct = { isCancelled: false };
setTimeout(function () {
    ct.isCancelled = true;
},
    30000);

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

let ais = [[{ Id: 1, Name: "Basic JS" }, basicAiJs ], [{Id: 2, Name: "Basic F#" }, play ]];
startGame(handleMessage, ct, 9, function (fun) {
    setTimeout(fun, 100);
}, ais);
