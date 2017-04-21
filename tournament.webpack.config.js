var path = require("path");
var webpack = require("webpack");

var cfg = {
    devtool: "source-map",
    entry: "./www/tournaments.offline.js",
    output: {
        path: path.join(__dirname, "www"),
        filename: "tournaments.js"
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