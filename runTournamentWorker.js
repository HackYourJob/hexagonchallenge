import { play, serializeEvents } from "./build/TournamentWorker";
import mysql from "mysql2";
import os from "os";
import zlib from 'zlib';

let workerName = 'worker-' + os.hostname();

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

let query = function(connection, sql, parameters) {
    return new Promise(function (resolve, reject) {
        connection.query(sql, parameters, function(err, result) {
            if (err) return reject(err)
            resolve(result)
        });
    });
};

function readFileAsync(file, encoding) {
    return new Promise(function (resolve, reject) {
        fs.readFile(file, encoding, function (err, data) {
            if (err) return reject(err) // rejects the promise with `err` as the reason
            resolve(data)               // fulfills the promise with `data` as the value
        })
    })
}

let saveResult = function(matchId, events, scores) {
    let connection = openConnection();

    events = zlib.gzipSync(events);

    let results = scores.map(function (s) { return { matchId: matchId, aiId: s.aiId, resources: s.resources, cells: s.cellsNb, bugs: s.bugs, order: s.position}});
    Promise.all(results.map(result => query(connection, 'INSERT INTO matchResult SET ?', result)))
        .then(() =>
            query(connection,
                `INSERT INTO matchEvents (matchId, events, processedAt) VALUES (?, ?, NOW())`,
                [matchId, events]))
        .then(() => {
            connection.end();
            console.log("match end : " + matchId);
            checkIfMatch();
            },
            err => {
                console.error(err);
                connection.end();
            });
};

let runMatch = function (ais, matchId) {
    let aiIdsByOrder = {};

    ais.map(r => aiIdsByOrder[r.order] = r.aiId);

    let oldLog = console.log
    console.log = function() {};

    let result;
    try {
        result = play(ais);
    } finally {
        console.log = oldLog;
    }
    
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

    query(connection,
            `SELECT matchId, tournamentName 
            FROM matchQueue
            WHERE lockedBy IS NULL
            ORDER BY createdAt ASC
            LIMIT 1`)
        .then(results => {
            if (!results || results.length != 1) {
                delayAction(checkIfMatch);
                return;
            }

            var match = results[0];

            return query(connection,
                    'UPDATE matchQueue SET lockedBy = ? WHERE matchId = ? AND lockedBy IS NULL',
                    [workerName, match.matchId])
                .then(results => {
                    return query(connection,
                        `SELECT matchQueue.lockedBy, matchAi.aiId, matchAi.order, ai.content, ai.ainame
                        FROM matchQueue
                        LEFT JOIN matchAi ON matchQueue.matchId = matchAi.matchId
                        LEFT JOIN ai ON ai.aiId = matchAi.aiId
                        WHERE matchQueue.matchId = ? AND matchQueue.lockedBy = ?`,
                        [match.matchId, workerName]);
                })
                .then(results => {
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
        })
        .then(() => connection.end(),
            err => {
                console.error(err);
                connection.end()
            });
};

checkIfMatch();
