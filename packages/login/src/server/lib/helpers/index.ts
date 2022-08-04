import path from "path";
import fs from "fs";

export const getAssets = (): assetsType => {
  const manifest = fs.readFileSync(
    path.join(__dirname, "client/manifest.json"),
    "utf-8"
  );

  const assets = JSON.parse(manifest);

  return assets;
};

export const getScripts = (assets: assetsType): string[] | void => {
  if (!assets || typeof assets !== "object") return;
  const regTest = /static\/js\/.*/;
  const keys = [];
  console.log(assets);
  for (let key in assets) {
    if (assets.hasOwnProperty(key) && regTest.test(key)) {
      keys.push(key);
    }
  }

  return keys;
};
