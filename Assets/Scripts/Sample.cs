#pragma warning disable 0649

using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UI;

public class Sample : MonoBehaviour
{
    [SerializeField] RawImage assetBundleHomeImage;
    [SerializeField] RawImage[] assetBundleIconsImages;
    [SerializeField] RawImage resourcesTitleImage;

    // Start is called before the first frame update
    void Start()
    {
        // 起動時に1回だけ設定する
        AssetLoadManager.ConvertPath = ConvertPath;

        // AssetBundle使用の有無
        // エディタでAssetBundleの動作テストをしたい時にtrueにする
        // 実機では問答無用でAssetBundleが使われる
        AssetLoader.UseAssetBundle = false;

        StartCoroutine(LoadAssetBundleHome());
        StartCoroutine(LoadAssetBundleIcons());
        StartCoroutine(LoadResourcesTitle());
    }

    /// <summary>
    /// エディタのファイルパスをAssetBundleのパスを変更する
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    static string ConvertPath(string path)
    {
        // 1AssetBundle1ファイルの場合はファイル名まで、1AssetBundle複数ファイルの場合はディレクトリ名までがpathに入ってくる
        // 「Assets/AssetBundle/Home/Bg.png」→「Assets/AssetBundle/Home/Bg.png」(1対1)
        // 「Assets/AssetBundle/Icon/Skill.png」→「Assets/AssetBundle/Home」(1対多)

        // 今回の例ではディレクトリを「_」で区切って小文字にしたものをファイル名としており、拡張子はunity3dに変更している
        // 例えばエディタ上にある「Assets/AssetBundle/Home/Bg.png」をAssetBundle化してアプリでDLした後に
        // Application.persistentDataPathのフォルダ以下に「assets_assetbundle_home_bg.unity3d」という名前で保存されている（という前提）
        // このパスコンバートは各自のアプリの作りに応じて変更してもらえばよい
#if UNITY_EDITOR
        var directory = "Assets/AssetBundleDownload";
#else
        var directory = Application.persistentDataPath;
#endif
        var filename = string.Join("_", path.Split(new[] { '\\', '/' }).ToArray()).ToLower();
        filename = $"{Path.GetFileNameWithoutExtension(filename)}.unity3d";
        return Path.Combine(directory, filename);
    }

    /// <summary>
    /// メモリ使用量が一定以上になったら解放
    /// </summary>
    static void UnusedUnload()
    {
        long memory = 0;
        if (Profiler.supported)
        {
            memory = Profiler.GetTotalAllocatedMemoryLong();
            if (memory < 100 * 1024 * 1024)
            {
                // メモリ使用量が100MBいないなら解放しない
                return;
            }
        }

        AssetLoadManager.UnusedUnload();
    }

    IEnumerator LoadAssetBundleHome()
    {
        var loader = AssetLoader.LoadAssetFile("Assets/AssetBundle/Home", "Bg.png");
        yield return loader;
        assetBundleHomeImage.texture = loader.Asset as Texture2D;
        assetBundleHomeImage.SetNativeSize();
        loader.Unload();
    }

    IEnumerator LoadAssetBundleIcons()
    {
        var loader = AssetLoader.LoadAssetFiles("Assets/AssetBundle/Icon", new[] { "Skill.png", "Item.png" });
        yield return loader;
        for (int i = 0; i < assetBundleIconsImages.Length; ++i)
        {
            assetBundleIconsImages[i].texture = loader.Assets[i] as Texture2D;
            assetBundleIconsImages[i].SetNativeSize();
        }
        loader.Unload();
    }

    IEnumerator LoadResourcesTitle()
    {
        var loader = AssetLoader.LoadAssetFile("Assets/Resources/Title", "Bg.png");
        yield return loader;
        resourcesTitleImage.texture = loader.Asset as Texture2D;
        resourcesTitleImage.SetNativeSize();
        loader.Unload();
    }
}

#pragma warning restore 0649
