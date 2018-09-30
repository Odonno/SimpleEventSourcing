const path = require('path');
const UglifyJSPlugin = require('uglifyjs-webpack-plugin');

const PRODUCTION = process.env.NODE_ENV == 'production';

module.exports = {
    entry: { 'main': './main.ts' },
    output: {
        path: path.resolve(__dirname, '../wwwroot/dist'),
        filename: 'bundle.min.js',
        publicPath: 'dist/'
    },
    devtool: 'source-map',
    module: {
        loaders: [
            {
                test: /\.tsx?$/,
                loader: 'awesome-typescript-loader'
            },
            {
                test: /\.(sass|css)$/,
                use: [
                    { loader: "style-loader" },
                    {
                        loader: "css-loader", options: {
                            minimize: PRODUCTION
                        }
                    },
                    { loader: "sass-loader" },
                ]
            },
            {
                test: /\.(jpg|png|woff(2)?|eot|ttf|svg)$/,
                use: {
                    loader: "url-loader",
                    options: {
                        limit: 25000,
                    },
                }
            },
            {
                test: /\.html$/,
                loader: 'html-loader'
            }
        ]
    },
    plugins: [
        new UglifyJSPlugin()
    ]
};