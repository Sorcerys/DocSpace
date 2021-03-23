const { CleanWebpackPlugin } = require("clean-webpack-plugin");
const CopyPlugin = require("copy-webpack-plugin");
const HtmlWebpackPlugin = require("html-webpack-plugin");
const ModuleFederationPlugin = require("webpack").container
  .ModuleFederationPlugin;
const TerserPlugin = require("terser-webpack-plugin");
const { InjectManifest } = require("workbox-webpack-plugin");
//const CompressionPlugin = require("compression-webpack-plugin");
const combineUrl = require("@appserver/common/utils/combineUrl");
const AppServerConfig = require("@appserver/common/constants/AppServerConfig");

const path = require("path");
const pkg = require("./package.json");
const deps = pkg.dependencies;
const homepage = pkg.homepage; //combineUrl(AppServerConfig.proxyURL, pkg.homepage);
const title = pkg.title;

var config = {
  mode: "development",
  entry: "./src/index",

  devServer: {
    publicPath: homepage,

    contentBase: [path.join(__dirname, "dist")],
    contentBasePublicPath: homepage,
    port: 5002,
    historyApiFallback: {
      // Paths with dots should still use the history fallback.
      // See https://github.com/facebook/create-react-app/issues/387.
      disableDotRule: true,
      index: homepage,
    },
    // proxy: [
    //   {
    //     context: "/api",
    //     target: "http://localhost:8092",
    //   },
    // ],
    hot: false,
    hotOnly: false,
    headers: {
      "Access-Control-Allow-Origin": "*",
      "Access-Control-Allow-Methods": "GET, POST, PUT, DELETE, PATCH, OPTIONS",
      "Access-Control-Allow-Headers":
        "X-Requested-With, content-type, Authorization",
    },
  },

  output: {
    publicPath: "auto",
    chunkFilename: "static/js/[id].[contenthash].js",
    //assetModuleFilename: "static/images/[hash][ext][query]",
    path: path.resolve(process.cwd(), "dist"),
    filename: "static/js/[name].[contenthash].bundle.js",
  },

  resolve: {
    extensions: [".jsx", ".js", ".json"],
    fallback: {
      crypto: false,
    },
  },

  module: {
    rules: [
      {
        test: /\.(png|jpe?g|gif|ico)$/i,
        type: "asset/resource",
        generator: {
          filename: "static/images/[hash][ext][query]",
        },
      },
      {
        test: /\.m?js/,
        type: "javascript/auto",
        resolve: {
          fullySpecified: false,
        },
      },
      {
        test: /\.react.svg$/,
        use: [
          {
            loader: "@svgr/webpack",
            options: {
              svgoConfig: {
                plugins: [{ removeViewBox: false }],
              },
            },
          },
        ],
      },
      { test: /\.json$/, loader: "json-loader" },
      {
        test: /\.css$/i,
        use: ["style-loader", "css-loader"],
      },
      {
        test: /\.s[ac]ss$/i,
        use: [
          // Creates `style` nodes from JS strings
          "style-loader",
          // Translates CSS into CommonJS
          "css-loader",
          // Compiles Sass to CSS
          "sass-loader",
        ],
      },
      {
        test: /\.(js|jsx)$/,
        exclude: /node_modules/,
        use: [
          {
            loader: "babel-loader",
            options: {
              presets: ["@babel/preset-react", "@babel/preset-env"],
              plugins: [
                "@babel/plugin-transform-runtime",
                "@babel/plugin-proposal-class-properties",
                "@babel/plugin-proposal-export-default-from",
              ],
            },
          },
          "source-map-loader",
        ],
      },
    ],
  },

  plugins: [
    new CleanWebpackPlugin(),
    new ModuleFederationPlugin({
      name: "people",
      filename: "remoteEntry.js",
      remotes: {
        studio: `studio@${combineUrl(
          AppServerConfig.proxyURL,
          "/remoteEntry.js"
        )}`,
        people: `people@${combineUrl(
          AppServerConfig.proxyURL,
          "/products/people/remoteEntry.js"
        )}`,
      },
      exposes: {
        "./app": "./src/People.jsx",
        "./GroupSelector": "./src/components/GroupSelector",
        "./PeopleSelector": "./src/components/PeopleSelector",
        "./PeopleSelector/UserTooltip":
          "./src/components/PeopleSelector/sub-components/UserTooltip.js",
      },
      shared: {
        ...deps,
        react: {
          singleton: true,
          requiredVersion: deps.react,
        },
        "react-dom": {
          singleton: true,
          requiredVersion: deps["react-dom"],
        },
      },
    }),
    new HtmlWebpackPlugin({
      template: "./public/index.html",
      publicPath: homepage,
      title: title,
      base: `${homepage}/`,
    }),
    new CopyPlugin({
      patterns: [
        {
          from: "public",
          globOptions: {
            dot: true,
            gitignore: true,
            ignore: ["**/index.html"],
          },
        },
      ],
    }),
  ],
};

module.exports = (env, argv) => {
  if (argv.mode === "production") {
    config.mode = "production";
    config.optimization = {
      splitChunks: { chunks: "all" },
      minimize: true,
      minimizer: [new TerserPlugin()],
    };
    config.plugins.push(
      new InjectManifest({
        mode: "production", //"development",
        swSrc: "./src/sw-template.js", // this is your sw template file
        swDest: "sw.js", // this will be created in the build step
        exclude: [/\.map$/, /manifest$/, /service-worker\.js$/],
      })
    );
    // config.plugins.push(
    //   new CompressionPlugin({
    //     filename: "[path][base].gz[query]",
    //     algorithm: "gzip",
    //     test: /\.js(\?.*)?$/i,
    //     threshold: 10240,
    //     minRatio: 0.8,
    //     deleteOriginalAssets: true,
    //   })
    // );
  } else {
    config.devtool = "cheap-module-source-map";
  }

  return config;
};
