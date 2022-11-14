const path = require("path");
const { BundleAnalyzerPlugin } = require("webpack-bundle-analyzer");
const MiniCssExtractPlugin = require("mini-css-extract-plugin");
const { CleanWebpackPlugin } = require("clean-webpack-plugin");

module.exports = (envVars) => {
    const { env, bundle_mode } = envVars;
    const webpack_dev = {
        mode: "development",
        devtool: "cheap-module-source-map",
    };
    const webpack_prod = {
        mode: "production",
        devtool: false,
    };
    const webpack_config = env === "dev" ? webpack_dev : webpack_prod;

    return {
        ...webpack_config,
        entry: [path.resolve(__dirname, "src/index.ts"), path.resolve(__dirname, "scss/main.scss")],
        output: {
            path: path.resolve(__dirname, "dist"),
            filename: "kpicommerce.bundle.js",
            publicPath: "/",
        },
        resolve: {
            extensions: [".scss", ".ts", ".js", ".jsx"],
        },
        devServer: {
            historyApiFallback: true,
        },
        module: {
            rules: [
                {
                    test: /\.(ts|js)x?$/,
                    exclude: /node_modules/,
                    use: [
                        {
                            loader: "babel-loader",
                        },
                    ],
                },
                {
                    test: /\.(c|s[ac])ss$/i,
                    use: [MiniCssExtractPlugin.loader, "css-loader", "sass-loader"],
                },
                {
                    test: /\.(png|svg|jpg|jpeg|gif)$/i,
                    type: "asset/resource",
                },
                {
                    test: /\.(woff(2)|eot|ttf|otf|svg)$/,
                    type: "asset/inline",
                },
            ],
        },

        plugins: [
            new CleanWebpackPlugin(),
            new MiniCssExtractPlugin({
                filename: "kpicommerce.bundle.css",
            }),
            new BundleAnalyzerPlugin({
                analyzerMode: bundle_mode === "on" ? "server" : "disabled",
            }),
        ],
    };
};
