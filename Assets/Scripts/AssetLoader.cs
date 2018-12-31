using UnityEngine;

public partial class AssetLoader : CustomYieldInstruction
{
    AssetLoadManager.ResourceInfo info;

    /// <summary>
    /// 単一ファイル
    /// </summary>
    public Object Asset { get; private set; }

    /// <summary>
    /// 複数ファイル
    /// </summary>
    public Object[] Assets { get; private set; }

    /// <summary>
    /// ロード中の有無
    /// </summary>
    public bool IsLoading { get; private set; }

    /// <summary>
    /// Resourcesのファイルの有無
    /// </summary>
    public bool IsResources { get; private set; }

    /// <summary>
    /// AssetBundle使用の有無
    /// 実機では強制的にAssetBundleが使われるようになる
    /// </summary>
    public static bool UseAssetBundle { get; set; }

    public override bool keepWaiting => IsLoading;

    /// <summary>
    /// 単体ファイルを読み込みます
    /// AssetBundleに使用する場合、1AssetBundle1ファイルのものにのみ使えます
    /// </summary>
    /// <param name="path">アセットバンドルのパス</param>
    /// <param name="name">アセット名</param>
    /// <returns></returns>
    public static AssetLoader LoadAssetFile(string path, string name)
    {
        AssetLoader loader = null;
        var resources = AssetLoadManager.IsResources(path);
        if (resources)
        {
            loader = LoadAssetFileResources(path, name);
        }
#if UNITY_EDITOR
        else if (!UseAssetBundle)
        {
            loader = LoadAssetFileEditor(path, name);
        }
#endif
        else
        {
            loader = LoadAssetFileAssetBundle(path, name);
        }
        loader.IsResources = resources;
        return loader;
    }

    /// <summary>
    /// 複数ファイルを複数読み込みます
    /// AssetBundleに使用する場合、1AssetBundle複数ファイルのものにのみ使えます
    /// </summary>
    /// <param name="path">アセットバンドルのパス</param>
    /// <param name="names">アセット名リスト</param>
    /// <returns></returns>
    public static AssetLoader LoadAssetFiles(string path, string[] names)
    {
        AssetLoader loader = null;
        var resources = AssetLoadManager.IsResources(path);
        if (resources)
        {
            loader = LoadAssetFilesResources(path, names);
        }
#if UNITY_EDITOR
        else if (!UseAssetBundle)
        {
            loader = LoadAssetFilesEditor(path, names);
        }
#endif
        else
        {
            loader = LoadAssetFilesAssetBundle(path, names);
        }

        loader.IsResources = resources;
        return loader;
    }

    /// <summary>
    /// 解放します
    /// </summary>
    public void Unload()
    {
        if (IsResources)
        {
            UnloadResources();
        }
#if UNITY_EDITOR
        else if (!UseAssetBundle)
        {
            UnloadEditor();
        }
#endif
        else
        {
            UnloadAssetBundle();
        }
    }
}
