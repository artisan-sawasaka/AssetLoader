using UnityEngine;

public partial class AssetLoader : CustomYieldInstruction
{
	static AssetLoader LoadAssetFileResources(string path, string name)
	{
		var loader = new AssetLoader
		{
			info = AssetLoadManager.Instance.LoadResourcesAsset(path, name),
			IsLoading = true,
		};
		loader.info.AddEndCallback((abi) => {
			loader.Asset = abi.asset;
			loader.IsLoading = false;
		});

		return loader;
	}

	static AssetLoader LoadAssetFilesResources(string path, string[] names)
	{
		var loader = new AssetLoader
		{
			info = AssetLoadManager.Instance.LoadResourcesAssets(path, names),
			IsLoading = true,
		};
		loader.info.AddEndCallback((abi) => {
            loader.Assets = abi.assets;
			loader.IsLoading = false;
		});

		return loader;
	}

	public void UnloadResources()
	{
		Asset = null;
		Assets = null;
		info = null;
	}
}
