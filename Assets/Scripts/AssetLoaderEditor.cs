using System.IO;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public partial class AssetLoader : CustomYieldInstruction
{
#if UNITY_EDITOR
	string directoryPath;
#endif

	static AssetLoader LoadAssetFileEditor(string path, string name)
	{
#if UNITY_EDITOR
		var loader = new AssetLoader
		{
			IsLoading = false,
		};
		loader.directoryPath = path;
		loader.Asset = AssetDatabase.LoadAssetAtPath<Object>(Path.Combine(path, name));

		return loader;
#else
		return null;
#endif
	}

	static AssetLoader LoadAssetFilesEditor(string path, string[] names)
	{
#if UNITY_EDITOR
		var loader = new AssetLoader {
			IsLoading = false,
		};
		loader.directoryPath = path;
		loader.Assets = new Object[names.Length];
		for (int i = 0; i < names.Length; ++i)
		{
			loader.Assets[i] = AssetDatabase.LoadAssetAtPath<Object>(Path.Combine(path, names[i]));
		}
		return loader;
#else
		return null;
#endif
	}

	void UnloadEditor()
	{
#if UNITY_EDITOR
		Asset = null;
		Assets = null;
#endif
	}
}
