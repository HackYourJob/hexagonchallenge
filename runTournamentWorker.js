import { play, serializeEvents } from "./build/TournamentWorker";
import mysql from "mysql2";

let workerName = 'worker1';

let openConnection = function() {
    return mysql.createConnection({
        host: process.env.MIXIT_HOST || 'database',
        port: process.env.MIXIT_PORT || '30101',
        user: process.env.MIXIT_LOGIN || 'mixit',
        database: process.env.MIXIT_DATABASE || 'mixit',
        password: process.env.MIXIT_PASSWORD || 'mixit'
    });
};

let delayAction = function(action) {
    setTimeout(action, 30000);
};

let saveResult = function(matchId, events, scores) {
    let connection = openConnection();

    let results = scores.map(function (s) { return { matchId: matchId, aiId: s.aiId, resources: s.resources, cells: s.cellsNb, bugs: s.bugs, order: s.position}});
    connection.query('INSERT INTO matchResult SET ?', results, function (err) {
        if (err) {
            console.error(err);
            return;
        }

        connection.query(`INSERT INTO matchEvents (matchId, events, processedAt) VALUES (?, ?, NOW())`, [matchId, events], function (err) {
            if (err) {
                console.error(err);
                return;
            }

            console.log("match end : " + matchId);

            checkIfMatch();
        });
    });
};

let runMatch = function (ais, matchId) {
    let aiIdsByOrder = {};

    ais.map(r => aiIdsByOrder[r.order] = r.aiId);

    let result = play(ais);

    var events = serializeEvents(result[0]);
    var scores = result[1].map(function(s, position) {
        return {
            position: position + 1,
            aiId: aiIdsByOrder[s[0]],
            cellsNb: s[1].CellsNb,
            resources: s[1].Resources,
            bugs: s[1].BugsNb
        }
    }).filter(s => s.aiId !== undefined).map(function (s, position) {
        s.position = position + 1
        return s;
    });
    console.log(scores)

    saveResult(matchId, events, scores)
};

function checkIfMatch() {
    let connection = openConnection();

    connection.query(
        `SELECT matchId, tournamentName 
        FROM matchQueue
        WHERE lockedBy IS NULL
        ORDER BY createdAt ASC
        LIMIT 1`,
        function (err, results) {
            if (err) {
                console.error(err);
            }

            if (!results || results.length != 1) {
                delayAction(checkIfMatch);
                return;
            }

            var match = results[0];

            connection.query(
                'UPDATE matchQueue SET lockedBy = ? WHERE matchId = ? AND lockedBy IS NULL',
                [workerName, match.matchId],
                function (err) {
                    if (err) {
                        console.error(err);
                    }

                    connection.query(
                        `SELECT matchQueue.lockedBy, matchAi.aiId, matchAi.order, ai.content, ai.ainame
                        FROM matchQueue
                        LEFT JOIN matchAi ON matchQueue.matchId = matchAi.matchId
                        LEFT JOIN ai ON ai.aiId = matchAi.aiId
                        WHERE matchQueue.matchId = ? AND matchQueue.lockedBy = ?`,
                        [match.matchId, workerName],
                        function (err, results) {
                            if (err) {
                                console.error(err);
                            }

                            if (!results || results.length < 1) {
                                checkIfMatch();
                                return;
                            }

                            console.log("Start match " + match.matchId);

                            let ais = results.map(function(r) {
                                return { aiId: r.aiId, order: r.order, code: r.content, name: r.ainame }
                            });
                            
                            runMatch(ais, match.matchId);
                        });
                });
        });
};

checkIfMatch();
