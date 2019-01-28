using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class AssetLoadManager : MonoBehaviour
{
	public class AssetBundleInfo
	{
		public AssetBundle ab;
		public int count;
		public bool locked;
	}

	public class ResourceInfo
	{
		public AssetBundleInfo assetBundleInfo;
		public string path;
		public UnityEngine.Object asset;
		public UnityEngine.Object[] assets;

		Action<ResourceInfo> callback;

		public void AddEndCallback(Action<ResourceInfo> loadEnd)
		{
			if (loadEnd != null)
			{
				callback += loadEnd;
			}
		}

		public void CallLoadEnd()
		{
			callback?.Invoke(this);
			callback = null;
		}
	}

	static readonly string RESOURCES_NAME = "Resources";
	static readonly string PATH_JOIN_CHAR = "/";
	static readonly char[] PATH_SPLIT = new[] { '\\', '/' };

	static AssetLoadManager instance;
	Dictionary<string, AssetBundleInfo> assetBundleInfos = new Dictionary<string, AssetBundleInfo>();

	/// <summary>
	/// シングルトン
	/// </summary>
	public static AssetLoadManager Instance
	{
		get
		{
			if (instance == null)
			{
				instance = FindObjectOfType<AssetLoadManager>();
				if (instance == null)
				{
					var obj = new GameObject(typeof(AssetLoadManager).ToString());
					instance = obj.AddComponent<AssetLoadManager>();
					DontDestroyOnLoad(obj);
				}
			}
			return instance;
		}
	}

	/// <summary>
	/// パスを変換するプロパティ
	/// DLCでパスを変更したい場合に設定する
	/// </summary>
	public static Func<string, string> ConvertPath { private get; set; } = path => path;

	/// <summary>
	/// Resourcesフォルダにあるか取得
	/// </summary>
	/// <param name="path">パスを指定</param>
	/// <returns>Resources以下にあればtrueを返す</returns>
	public static bool IsResources(string path)
	{
		return path != CreateResourcesDirectoryPath(path);
	}

	/// <summary>
	/// 使用済みリソース解放
	/// バトルなど大きいシーンの前に呼ぶ
	/// </summary>
	public static void UnusedUnload()
	{
#if UNITY_EDITOR
		AssetDatabase.RemoveUnusedAssetBundleNames();
#endif
		Resources.UnloadUnusedAssets();
		GC.Collect();
	}

	static string CreateResourcesDirectoryPath(string path)
	{
		var ss = path.Split(PATH_SPLIT);
		for (int i = 0; i < ss.Length; ++i)
		{
			if (ss[i] == RESOURCES_NAME)
			{
				var ts = ss.Skip(i + 1).ToArray();
				return string.Join(PATH_JOIN_CHAR, ts);
			}
		}
		return path;
	}

	public ResourceInfo LoadAssetBundleAsset(string path, string name)
	{
		var info = GetResourceInfo(Path.Combine(path, name));
		StartCoroutine(LoadAssetBundleAssetData(info, name));

		return info;
	}

	public ResourceInfo LoadAssetBundleAssets(string path, string[] names)
	{
		var info = GetResourceInfo(path);
		StartCoroutine(LoadAssetBundleAssetsData(info, names));

		return info;
	}

	public void UnloadAssetBundle(string path)
	{
		if (!assetBundleInfos.TryGetValue(path, out var info))
		{
			return;
		}

		if (--info.count <= 0)
		{
			if (!info.locked)
			{
				info.ab.Unload(false);
				assetBundleInfos.Remove(path);
			}
			else
			{
				StartCoroutine(UnloadAssetBundleData(info, path));
			}
		}
	}

	public ResourceInfo LoadResourcesAsset(string path, string name)
	{
		var info = new ResourceInfo
		{
			path = path,
		};
		StartCoroutine(LoadResourcesAssetData(info, name));

		return info;
	}

	public ResourceInfo LoadResourcesAssets(string path, string[] names)
	{
		var info = new ResourceInfo
		{
			path = path,
		};
		StartCoroutine(LoadResourcesAssetsData(info, names));

		return info;
	}

	ResourceInfo GetResourceInfo(string path)
	{
		var resourceInfo = new ResourceInfo
		{
			path = path,
		};

		if (!assetBundleInfos.TryGetValue(path, out var info))
		{
			info = new AssetBundleInfo();
			assetBundleInfos.Add(path, info);
		}
		resourceInfo.assetBundleInfo = info;
		return resourceInfo;
	}

	IEnumerator LoadAssetBundleBase(AssetBundleInfo info, string path)
	{
		if (info.count > 0)
		{
			info.count++;
			while (info.locked) yield return null;
			info.locked = true;
		}
		else
		{
			info.count++;
			info.locked = true;
			var request = AssetBundle.LoadFromFileAsync(ConvertPath(path));
			yield return request;
			info.ab = request.assetBundle;
		}
	}

	IEnumerator LoadAssetBundleAssetData(ResourceInfo info, string name)
	{
		var assetBundleInfo = info.assetBundleInfo;
		yield return LoadAssetBundleBase(assetBundleInfo, info.path);

		var request = assetBundleInfo.ab.LoadAssetAsync<UnityEngine.Object>(name);
		yield return request;
		info.asset = request.asset;

		info.CallLoadEnd();
		assetBundleInfo.locked = false;
	}

	IEnumerator LoadAssetBundleAssetsData(ResourceInfo info, string[] names)
	{
		var assetBundleInfo = info.assetBundleInfo;
		yield return LoadAssetBundleBase(assetBundleInfo, info.path);

		info.assets = new UnityEngine.Object[names.Length];
        var cl = new CoroutineList();
        for (int i = 0; i < names.Length; ++i)
        {
            cl.Add(LoadAssetBundleAssetsData(assetBundleInfo.ab, names[i], info.assets, i));
        }
        yield return cl.WaitForCoroutine(this);

        info.CallLoadEnd();
		assetBundleInfo.locked = false;
	}

    IEnumerator LoadAssetBundleAssetsData(AssetBundle ab, string name, UnityEngine.Object[] assets, int index)
    {
        var request = ab.LoadAssetAsync<UnityEngine.Object>(name);
        yield return request;
        assets[index] = request.asset;
    }

    IEnumerator UnloadAssetBundleData(AssetBundleInfo info, string path)
	{
		while (info.locked) yield return null;
		info.ab.Unload(false);
		assetBundleInfos.Remove(path);
	}

	IEnumerator LoadResourcesAssetData(ResourceInfo info, string name)
	{
		var path = CreateResourcesDirectoryPath(info.path);
		var request = Resources.LoadAsync(Path.Combine(path, Path.GetFileNameWithoutExtension(name)));
		yield return request;
		info.asset = request.asset;

		info.CallLoadEnd();
	}

	IEnumerator LoadResourcesAssetsData(ResourceInfo info, string[] names)
	{
		var path = CreateResourcesDirectoryPath(info.path);
		info.assets = new UnityEngine.Object[names.Length];
		var cl = new CoroutineList();
		for (int i = 0; i < names.Length; ++i)
		{
			var co = LoadResourcesAssetData(
				Path.Combine(path, Path.GetFileNameWithoutExtension(names[i])),
				info.assets, i);
			cl.Add(co);
		}
		yield return cl.WaitForCoroutine(this);

		info.CallLoadEnd();
	}

	IEnumerator LoadResourcesAssetData(string path, UnityEngine.Object[] assets, int index)
	{
		var request = Resources.LoadAsync(path);
		yield return request;
		assets[index] = request.asset;
	}
}
