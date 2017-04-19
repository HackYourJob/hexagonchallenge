var path = require("path");
var webpack = require("webpack");
var fs = require("fs");

var nodeModules = {};
fs.readdirSync('node_modules')
    .filter(function (x) {
        return ['.bin'].indexOf(x) === -1;
    })
    .forEach(function (mod) {
        nodeModules[mod] = 'commonjs ' + mod;
    });

var cfg = {
    devtool: "source-map",
    entry: "./runTournamentWorker.js",
    target: 'node',
    output: {
        path: path.join(__dirname, "build"),
        filename: "runTournamentWorker.js"
    },
    module: {
        rules: [
          {
              test: /\.js$/,
              use: ["source-map-loader"],
              enforce: "pre"
          }
        ]
    },
    node: {
        fs: 'empty'
    }
};

module.exports = cfg;