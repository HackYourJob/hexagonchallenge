var path = require("path");
var webpack = require("webpack");

var cfg = {
    devtool: "source-map",
    entry: "./runTournamentWorker.js",
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
    }
};

module.exports = cfg;