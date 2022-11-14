const path = require('path');
const MiniCssExtractPlugin = require('mini-css-extract-plugin');

module.exports = (env, argv) => {

  return {
    devtool: 'inline-source-map',
    resolve: {
      extensions: ['.ts', '.tsx', '.js', '.jsx'],
      alias: {
        '@epi/components': path.resolve(__dirname, 'packages/components/src'),
        'remote-component.config.js': __dirname + '/remote-component.config.js'
      }
    },
    module: {
      rules: [
        {
          test: /\.tsx?$/,
          exclude: /node_modules/,
          use: [
            {
              loader: 'ts-loader',
              options: {
                transpileOnly: true
              }
            }
          ]
        },
        {
          test: /\.m?js$/,
          exclude: /node_modules/,
          use: {
            loader: "babel-loader",
            options: {
              presets: [
                         [
                         "@babel/env",
                         {
                           useBuiltIns: "entry"
                         }
                        ],
                        "@babel/react"
                      ]
                  }
            }
        },
        {
          test: /\.s?css$/,
          use: [
            {
              loader: argv.mode === "production" ? MiniCssExtractPlugin.loader : 'style-loader',
            },
            {
              loader: 'css-loader',
            },
            {
              loader: 'sass-loader',
              options: {
                sassOptions: {
                  includePaths: ['node_modules'],
                }
              }
            }
          ]
        }
      ]
    }
  }
}
