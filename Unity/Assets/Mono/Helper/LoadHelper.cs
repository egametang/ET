using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ET
{
    public static class LoadHelper
    {
        public static TextAsset LoadTextAsset(string path) 
        {
            if (Application.isEditor)
            {
                TextAsset resource = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path) as TextAsset;
                return resource ;
            }

            path = path + ".unity3d";
            string p = Path.Combine(PathHelper.AppHotfixResPath, path);
            AssetBundle assetBundle = null;
            if (File.Exists(p))
            {
                assetBundle = AssetBundle.LoadFromFile(p);
            }
            else
            {
                p = Path.Combine(PathHelper.AppResPath, path);
                assetBundle = AssetBundle.LoadFromFile(p);
            }

            if (assetBundle == null)
            {
                // 获取资源的时候会抛异常，这个地方不直接抛异常，因为有些地方需要Load之后判断是否Load成功
                Debug.LogError($"assets bundle not found: {path}");
                return default(TextAsset);
            }
            return assetBundle.LoadAsset(path)  as  TextAsset;
        }


        public static UnityEngine.Object LoadAssetAtPath(string assetPath)
        {
            return AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);
        }

        public static  int GetAssetPathsFromAssetBundleCount(string assetBundleName)
        {
            string[] resultArr =  AssetDatabase.GetAssetPathsFromAssetBundle(assetBundleName);
            return resultArr.Length;
        }
        
        public static  string[] GetAssetPathsFromAssetBundle(string assetBundleName)
        {
            return AssetDatabase.GetAssetPathsFromAssetBundle(assetBundleName);
        }
        
        
        public static int GetAssetBundleDependenciesCount(string assetBundleName, bool recursive)
        {
            string[] result =  AssetDatabase.GetAssetBundleDependencies(assetBundleName, recursive);
            return result.Length;
        }
        

        public static  string[] GetAssetBundleDependencies(string assetBundleName, bool recursive)
        {
            return  AssetDatabase.GetAssetBundleDependencies(assetBundleName, recursive);
        }


        public static int GetanimationClipsLength(this Animator self )
        {
            return self.runtimeAnimatorController.animationClips.Length;
        }

        public static int GetAnimatorControllerParameterLength(this Animator self )
        {
            return self.parameters.Length;
        }


    }
}