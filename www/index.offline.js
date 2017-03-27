import { startGame } from './fable/Game';
import { handleMessage } from './fable/UI';

let ct = { isCancelled: false };
setTimeout(function () {
    ct.isCancelled = true;
},
    30000);
startGame(handleMessage, ct, 5, function (fun) {
    setTimeout(fun, 100);
});
