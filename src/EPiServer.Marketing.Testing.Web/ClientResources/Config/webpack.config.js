const MiniCssExtractPlugin = require('mini-css-extract-plugin');
const { CleanWebpackPlugin } = require('clean-webpack-plugin');
const BundleAnalyzerPlugin = require('webpack-bundle-analyzer').BundleAnalyzerPlugin;
const path = require( 'path' )
module.exports = (env, argv) => {    
    const webpackCommon = require("./webpack.common")(env, argv);
    return {
        ...webpackCommon,
        entry: `./src/index.tsx`,
        output: {
            filename: `abtestingconfig.bundle.js`,
        },
        plugins: [
            new MiniCssExtractPlugin({
                filename: 'abtestingconfig.bundle.css'
            }),
            new CleanWebpackPlugin(),
            new BundleAnalyzerPlugin({ analyzerMode: "disabled" })
        ]
    };
};
