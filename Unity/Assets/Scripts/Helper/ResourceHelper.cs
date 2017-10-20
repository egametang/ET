using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Model
{
    public class ResourceHelper 
    {
        private static Dictionary<string, AssetBundle> AB = new Dictionary<string, AssetBundle>();

        /// <summary>
        /// 加载AssetBundle返回GameObject
        /// </summary>
        public static UnityEngine.Object LoadResource(string assetName, string prefabName)
        {
            AssetBundle ab = LoadAB(assetName);
            var assetLoadRequest = ab.LoadAssetAsync<GameObject>(prefabName);
            return assetLoadRequest.asset as GameObject;
        }

        /// <summary>
        /// 加载AssetBundle返回TextAsset
        /// </summary>
        public static TextAsset LoadTextAsset(string assetName, string prefabName)
        {
            AssetBundle ab = LoadAB(assetName);
            TextAsset textAsset = ab.LoadAsset<TextAsset>(prefabName);
            return textAsset;
        }

        /// <summary>
        /// 加载AssetBundle返回AssetBundle
        /// </summary>
        public static AssetBundle LoadAB(string assetName)
        {
            assetName = assetName.ToLower() + AppConst.AssetBundleExtendName;
            string url = PathHelp.AppHotfixResPath + assetName;
            AssetBundle bundle = null;
            AB.TryGetValue(assetName, out bundle);
            if (bundle == null)
            {
                var www = AssetBundle.LoadFromFileAsync(url);
                bundle = www.assetBundle;
                AB.Add(assetName, bundle);
            }
            return bundle;
        }

        //public static ResourceHelper instance;
        //private string[] m_Variants = { };
        //private AssetBundleManifest manifest;
        //private AssetBundle shared = null;
        //private AssetBundle assetbundle = null;
        //private Dictionary<string, AssetBundle> bundles;

        //void Awake()
        //{
        //    instance = this;
        //    // Initialize();
        //}

        ///// <summary>
        ///// 初始化
        ///// </summary>
        //public void Initialize()
        //{
        //    byte[] stream = null;
        //    string uri = string.Empty;
        //    bundles = new Dictionary<string, AssetBundle>();
        //    uri = PathHelp.AppHotfixResPath + AppConst.StreamingAssetsName;
        //    if (!File.Exists(uri))
        //        return;
        //    stream = File.ReadAllBytes(uri);
        //    assetbundle = AssetBundle.LoadFromMemory(stream);
        //    //assetbundle = AssetBundle.LoadFromMemoryAsync(stream).assetBundle;
        //    manifest = assetbundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
        //}

        ///// <summary>
        /////同步加载资源
        ///// </summary>
        //public GameObject LoadAsset(string abname, string assetname)
        //{
        //    Log.Info($"{"加载资源"}{abname}{assetname}");
        //    abname = abname.ToLower();
        //    AssetBundle bundle = LoadAssetBundle(abname);
        //    return bundle.LoadAsset<GameObject>(assetname);
        //}

        ///// <summary>
        ///// 载入素材并且创建
        ///// </summary>
        //public GameObject LoadAssetAndCreate(string abname, string assetname)
        //{
        //    abname = abname.ToLower();
        //    AssetBundle bundle = LoadAssetBundle(abname);
        //    GameObject obj = bundle.LoadAsset<GameObject>(assetname);
        //    GameObject _obj = GameObject.Instantiate(obj);
        //    return _obj;
        //}

        //public UnityEngine.Object LoadAssetObject(string abname, string assetname)
        //{
        //    abname = abname.ToLower();
        //    AssetBundle bundle = LoadAssetBundle(abname);
        //    return bundle.LoadAsset<UnityEngine.Object>(assetname);
        //}

        //public Material LoadAssetMaterial(string abname, string materialName)
        //{
        //    abname = abname.ToLower();
        //    AssetBundle bundle = LoadAssetBundle(abname);
        //    return bundle.LoadAsset<Material>(materialName);
        //}

        //public Texture LoadAssetTexture(string abname, string textureName)
        //{
        //    abname = abname.ToLower();
        //    AssetBundle bundle = LoadAssetBundle(abname);
        //    return bundle.LoadAsset<Texture2D>(textureName);
        //}
        //public byte[] LoadTextAsset(string abname, string TextAssetName)
        //{
        //    abname = abname.ToLower();
        //    AssetBundle bundle = LoadAssetBundle(abname);
        //    return bundle.LoadAsset<TextAsset>(TextAssetName).bytes;
        //}



        ///// <summary>
        ///// 载入AssetBundle
        ///// </summary>
        //AssetBundle LoadAssetBundle(string abname)
        //{
        //    if (!abname.EndsWith(AppConst.AssetBundleExtendName))
        //    {
        //        abname += AppConst.AssetBundleExtendName;
        //    }
        //    AssetBundle bundle = null;
        //    if (!bundles.ContainsKey(abname))
        //    {
        //        byte[] stream = null;
        //        string uri = PathHelp.AppHotfixResPath + abname;
        //        Log.Debug("LoadFile::>> " + uri);
        //        LoadDependencies(abname);
        //        stream = File.ReadAllBytes(uri);
        //        //bundle = AssetBundle.LoadFromMemory(stream);
        //        bundle = AssetBundle.LoadFromMemoryAsync(stream).assetBundle;
        //        bundles.Add(abname, bundle);
        //    }
        //    else
        //    {
        //        bundles.TryGetValue(abname, out bundle);
        //    }
        //    return bundle;
        //}


        //public void LoadSceneAsync(string sceneName, string _hash, AsyncOperation operation, int mod = 1)
        //{
        //    StartCoroutine(LoadAssetBundleSceneAsync(sceneName, _hash, operation, mod));
        //}

        //private IEnumerator LoadAssetBundleSceneAsync(string sceneName, string _hash, AsyncOperation operation, int mode)
        //{
        //    Log.Debug("异步加载场景：" + sceneName);
        //    LoadSceneMode emMode = (LoadSceneMode)mode;
        //    Hash128 hash = new Hash128();
        //    if (_hash != null) { Hash128.Parse(_hash); }
        //    string url = PathHelp.AppHotfixResPath + sceneName;
        //    Log.Debug("url==" + url);
        //    WWW www = null;
        //    if (_hash != null)
        //    {
        //        www = WWW.LoadFromCacheOrDownload(url, hash);
        //    }
        //    else
        //    {
        //        www = WWW.LoadFromCacheOrDownload(url, 1);
        //    }
        //    yield return www;
        //    if (www.isDone && !string.IsNullOrEmpty(www.error))
        //    {
        //        Debug.LogError("异步加载 scene error：" + www.error);
        //        yield break;
        //    }
        //    var bundle = www.assetBundle;
        //    AsyncOperation apt = SceneManager.LoadSceneAsync(sceneName, emMode);
        //    operation = apt;
        //}

        //public void LoadAssetCacheAsync(string abName, string name, string _hash, GameObject LoadPrefab, bool isNew = true)
        //{
        //    StartCoroutine(LoadAssetBundleCache(abName, name, _hash, LoadPrefab, isNew));
        //}
        //IEnumerator LoadAssetBundleCache(string abName, string name, string _hash, GameObject LoadPrefab, bool isNew)
        //{
        //    abName = abName + AppConst.AssetBundleExtendName;
        //    string m_BaseDownloadingURL = PathHelp.AppHotfixResPath;
        //    string url = m_BaseDownloadingURL + abName;
        //    AssetBundle LoadAssetObj = null;
        //    Hash128 hash = new Hash128();
        //    if (_hash != null) { hash = Hash128.Parse(_hash); }
        //    if (!bundles.ContainsKey(abName))
        //    {
        //        WWW www = null;
        //        if (_hash != null)
        //        {
        //            Log.Debug("url==" + url + " hash=" + _hash);
        //            www = WWW.LoadFromCacheOrDownload(url, hash);
        //        }
        //        else
        //        {
        //            Log.Debug("url==" + url + " version=" + 1);
        //            www = WWW.LoadFromCacheOrDownload(url, 1);
        //        }
        //        yield return www;
        //        if (www.isDone && !string.IsNullOrEmpty(www.error))
        //        {
        //            Debug.LogError("异步加载assetBundle error：" + www.error);
        //            yield break;
        //        }
        //        LoadAssetObj = www.assetBundle;
        //        if (LoadAssetObj == null)
        //        {
        //            Debug.LogError("Failed to load AssetBundle!");
        //            yield break;
        //        }
        //        bundles.Add(abName, LoadAssetObj);
        //    }
        //    else
        //    {
        //        bundles.TryGetValue(abName, out LoadAssetObj);
        //    }
        //    AssetBundleRequest assetLoadRequest;
        //    yield return assetLoadRequest = LoadAssetObj.LoadAssetAsync<GameObject>(name);
        //    GameObject prefab = assetLoadRequest.asset as GameObject;
        //    if (!isNew) { yield return LoadPrefab = prefab; }
        //    else { yield return LoadPrefab = Instantiate(prefab); }

        //}

        ///// <summary>
        ///// 载入依赖
        ///// </summary>
        //void LoadDependencies(string name)
        //{
        //    if (manifest == null)
        //    {
        //        Debug.LogError("Please initialize AssetBundleManifest by calling AssetBundleManager.Initialize()");
        //        return;
        //    }
        //    // Get dependecies from the AssetBundleManifest object..
        //    string[] dependencies = manifest.GetAllDependencies(name);
        //    if (dependencies.Length == 0)
        //        return;

        //    for (int i = 0; i < dependencies.Length; i++)
        //        dependencies[i] = RemapVariantName(dependencies[i]);

        //    // Record and load all dependencies.
        //    for (int i = 0; i < dependencies.Length; i++)
        //    {
        //        LoadAssetBundle(dependencies[i]);
        //    }
        //}

        //// Remaps the asset bundle name to the best fitting asset bundle variant.
        //string RemapVariantName(string assetBundleName)
        //{
        //    string[] bundlesWithVariant = manifest.GetAllAssetBundlesWithVariant();

        //    // If the asset bundle doesn't have variant, simply return.
        //    if (System.Array.IndexOf(bundlesWithVariant, assetBundleName) < 0)
        //        return assetBundleName;

        //    string[] split = assetBundleName.Split('.');

        //    int bestFit = int.MaxValue;
        //    int bestFitIndex = -1;
        //    // Loop all the assetBundles with variant to find the best fit variant assetBundle.
        //    for (int i = 0; i < bundlesWithVariant.Length; i++)
        //    {
        //        string[] curSplit = bundlesWithVariant[i].Split('.');
        //        if (curSplit[0] != split[0])
        //            continue;

        //        int found = System.Array.IndexOf(m_Variants, curSplit[1]);
        //        if (found != -1 && found < bestFit)
        //        {
        //            bestFit = found;
        //            bestFitIndex = i;
        //        }
        //    }
        //    if (bestFitIndex != -1)
        //        return bundlesWithVariant[bestFitIndex];
        //    else
        //        return assetBundleName;
        //}


        //public void OnCler()
        //{
        //    if (assetbundle != null)
        //        assetbundle.Unload(false);
        //    Resources.UnloadUnusedAssets();
        //    System.GC.Collect();
        //}

        //public void Unload(string abname)
        //{
        //    if (abname.Equals(null))
        //        return;
        //    abname += AppConst.AssetBundleExtendName;
        //    if (bundles.ContainsKey(abname))
        //    {
        //        bundles[abname].Unload(true);
        //        bundles.Remove(abname);
        //        Debug.Log("Unload：" + abname);
        //    }
        //}

        ///// <summary>
        ///// 销毁资源
        ///// </summary>
        //void OnDestroy()
        //{
        //    OnCler();
        //    if (shared != null)
        //        shared.Unload(true);
        //    if (manifest != null)
        //        manifest = null;
        //    if (bundles == null)
        //        return;
        //    foreach (string key in bundles.Keys)
        //    {
        //        AssetBundle ab;
        //        bundles.TryGetValue(key, out ab);
        //        ab.Unload(true);
        //    }
        //    bundles.Clear();
        //}

    }
}
