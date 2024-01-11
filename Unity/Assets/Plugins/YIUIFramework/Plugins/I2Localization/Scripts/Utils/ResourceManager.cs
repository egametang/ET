using UnityEngine;
using System.Collections.Generic;
using YIUIFramework;
#if UNITY_5_4_OR_NEWER
using UnityEngine.SceneManagement;
#endif

namespace I2.Loc
{
    public interface IResourceManager_Bundles
    {
        Object LoadFromBundle(string path, System.Type assetType);
    }

    public class ResourceManager : MonoBehaviour
    {
        #region Singleton

        public static ResourceManager pInstance
        {
            get
            {
                bool changed = mInstance == null;

                if (mInstance == null)
                    mInstance = (ResourceManager)FindObjectOfType(typeof(ResourceManager));

                if (mInstance == null)
                {
                    GameObject GO = new GameObject("I2ResourceManager", typeof(ResourceManager));
                    GO.hideFlags =
                        GO.hideFlags | HideFlags.HideAndDontSave; // Only hide it if this manager was autocreated
                    mInstance = GO.GetComponent<ResourceManager>();
                    #if UNITY_5_4_OR_NEWER
                    SceneManager.sceneLoaded += MyOnLevelWasLoaded;
                    #endif
                }

                if (changed && Application.isPlaying)
                    DontDestroyOnLoad(mInstance.gameObject);

                return mInstance;
            }
        }

        static ResourceManager mInstance;

        #endregion

        #region Management

        public List<IResourceManager_Bundles> mBundleManagers = new List<IResourceManager_Bundles>();

        #if UNITY_5_4_OR_NEWER
        public static void MyOnLevelWasLoaded(Scene scene, LoadSceneMode mode)
            #else
		public void OnLevelWasLoaded()
            #endif
        {
            pInstance.CleanResourceCache();
            LocalizationManager.UpdateSources();
        }

        #endregion

        #region Assets

        public Object[] Assets;

        // This function tries finding an asset in the Assets array, if not found it tries loading it from the Resources Folder
        public T GetAsset<T>(string Name) where T : Object
        {
            T Obj = FindAsset(Name) as T;
            if (Obj != null)
                return Obj;

            return LoadFromResources<T>(Name);
        }

        Object FindAsset(string Name)
        {
            if (Assets != null)
            {
                for (int i = 0, imax = Assets.Length; i < imax; ++i)
                    if (Assets[i] != null && Assets[i].name == Name)
                        return Assets[i];
            }

            return null;
        }

        public bool HasAsset(Object Obj)
        {
            if (Assets == null)
                return false;
            return System.Array.IndexOf(Assets, Obj) >= 0;
        }

        #endregion

        #region Resources Cache

        // This cache is kept for a few moments and then cleared
        // Its meant to avoid doing several Resource.Load for the same Asset while Localizing 
        // (e.g. Lot of labels could be trying to Load the same Font)
        //这个缓存被保留了一会儿，然后被清除
        //这意味着避免做几个资源。在本地化时加载相同的资源
        //(例如，许多标签可能试图加载相同的字体)
        readonly Dictionary<string, Object> mResourcesCache =
            new Dictionary<string, Object>(System.StringComparer
                                                 .Ordinal);

        // This is used to avoid re-loading the same object from resources in the same frame

        //bool mCleaningScheduled = false;

        public T LoadFromResources<T>(string Path) where T : Object
        {
            try
            {
                if (string.IsNullOrEmpty(Path))
                    return null;

                Object Obj;

                // Doing Resource.Load is very slow so we are catching the recently loaded objects
                if (mResourcesCache.TryGetValue(Path, out Obj) && Obj != null)
                {
                    return Obj as T;
                }

                T obj = null;

                if (Path.EndsWith("]",
                        System.StringComparison
                              .OrdinalIgnoreCase)) // Handle sprites (Multiple) loaded from resources :   "SpritePath[SpriteName]"
                {
                    int    idx             = Path.LastIndexOf("[", System.StringComparison.OrdinalIgnoreCase);
                    int    len             = Path.Length - idx - 2;
                    string MultiSpriteName = Path.Substring(idx + 1, len);
                    Path = Path.Substring(0, idx);

                    T[] objs = Resources.LoadAll<T>(Path);
                    for (int j = 0, jmax = objs.Length; j < jmax; ++j)
                        if (objs[j].name.Equals(MultiSpriteName))
                        {
                            obj = objs[j];
                            break;
                        }
                }
                else
                {
                    obj = Resources.Load(Path, typeof(T)) as T;
                }

                if (obj == null)
                    obj = LoadFromBundle<T>(Path);

                if (obj != null)
                    mResourcesCache[Path] = obj;

                /*if (!mCleaningScheduled)
                {
                    Invoke("CleanResourceCache", 0.1f);
                    mCleaningScheduled = true;
                }*/
                //if (obj==null)
                //Debug.LogWarningFormat( "Unable to load {0} '{1}'", typeof( T ), Path );

                return obj;
            }
            catch (System.Exception e)
            {
                Debug.LogErrorFormat("Unable to load {0} '{1}'\nERROR: {2}", typeof(T), Path, e.ToString());
                return null;
            }
        }

        public T LoadFromBundle<T>(string path) where T : Object
        {
            for (int i = 0, imax = mBundleManagers.Count; i < imax; ++i)
                if (mBundleManagers[i] != null)
                {
                    var obj = mBundleManagers[i].LoadFromBundle(path, typeof(T)) as T;
                    if (obj != null)
                        return obj;
                }

            return null;
        }

        public void CleanResourceCache(bool unloadResources = false)
        {
            Debug.Log("I2 CleanResourceCache");

            foreach (var obj in mResourcesCache.Values)
            {
                YIUILoadHelper.Release(obj);
            }

            mResourcesCache.Clear();
            if (unloadResources)
                Resources.UnloadUnusedAssets();

            CancelInvoke();

            //mCleaningScheduled = false;
        }

        #endregion
    }
}