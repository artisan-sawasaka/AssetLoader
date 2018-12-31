using UnityEngine;

public partial class AssetLoader : CustomYieldInstruction
{
	static AssetLoader LoadAssetFileAssetBundle(string path, string name)
	{
		var loader = new AssetLoader {
			IsLoading = true,
		};
		loader.info = AssetLoadManager.Instance.LoadAssetBundleAsset(path, name);
		loader.info.AddEndCallback(info => {
			loader.Asset = info.asset;
			loader.IsLoading = false;
		});

		return loader;
	}

	static AssetLoader LoadAssetFilesAssetBundle(string path, string[] names)
	{
		var loader = new AssetLoader {
			IsLoading = true,
		};
		loader.info = AssetLoadManager.Instance.LoadAssetBundleAssets(path, names);
		loader.info.AddEndCallback(info => {
			loader.Assets = info.assets;
			loader.IsLoading = false;
		});

		return loader;
	}

	void UnloadAssetBundle()
	{
		if (info == null)
		{
			return;
		}

		Asset = null;
		Assets = null;
		AssetLoadManager.Instance.UnloadAssetBundle(info.path);
		info = null;
	}
}
