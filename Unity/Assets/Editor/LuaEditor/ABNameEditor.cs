using System.Collections.Generic;
using System.IO;
using UnityEditor;

namespace ET
{
    public class ABNameEditor: Editor
    {
        public static string unity3dEx = ".unity3d";

        public static string dependentABPrefix = "assets_";

        private static void SetAssetName(UnityEngine.Object go)
        {
            AssetImporter assetImporter = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(go.GetInstanceID()));

            assetImporter.assetBundleName = $"{go.name.ToLower()}{unity3dEx}";
        }

        // [MenuItem("Tools/AssetBundle/设置选中项的AB名为文件名，并重设依赖AB名")]
        public static void SetSelectionABName()
        {
            var gameObjects = Selection.objects;

            foreach (UnityEngine.Object go in gameObjects)
            {
                SetAssetName(go);
            }

            AnalysisAssetBundleNames();

            RemoveUnusedAssetBundleNames();
        }

        // [MenuItem("Tools/AssetBundle/设置选中项的AB名为FUI文件名，并重设依赖AB名")]
        public static void SetSelectionFUIABName()
        {
            var gameObjects = Selection.objects;

            foreach (UnityEngine.Object go in gameObjects)
            {
                AssetImporter assetImporter = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(go.GetInstanceID()));

                if (go.name.Contains("fui"))
                {
                    assetImporter.assetBundleName = $"{go.name.ToLower()}{unity3dEx}";
                }
                else
                {
                    string[] texts = go.name.Split('_');

                    if (texts != null && texts.Length > 0)
                    {
                        assetImporter.assetBundleName = $"{texts[0].ToLower()}{unity3dEx}";
                    }
                }
            }

            AnalysisAssetBundleNames();

            RemoveUnusedAssetBundleNames();
        }

        //[MenuItem("Tools/AssetBundle/设置选中Lua文件夹的子文件的AB名为文件夹名，并重设依赖AB名")]
        public static void SetSelectionLuaABName()
        {
            string[] assetGUIDs = Selection.assetGUIDs;

            foreach (string guid in assetGUIDs)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);

                SetFolderLuaABName(assetPath);
            }

            AnalysisAssetBundleNames();

            RemoveUnusedAssetBundleNames();
        }

        public static void SetFolderLuaABName(string folderPath)
        {
            if (Directory.Exists(folderPath))
            {
                string dirName = folderPath.Substring(folderPath.LastIndexOf("/") + 1);
                string[] filePath = Directory.GetFiles(folderPath, "*.lua.txt", SearchOption.AllDirectories);

                foreach (string item in filePath)
                {
                    AssetImporter assetImporter = AssetImporter.GetAtPath(item);

                    if (assetImporter)
                    {
                        assetImporter.assetBundleName = $"{dirName.ToLower()}_lua{unity3dEx}";
                    }
                }
            }
        }

        // [MenuItem("Tools/AssetBundle/设置选中项的AB名为None，并重设依赖AB名")]
        public static void SetSelectionABNameNone()
        {
            var gameObjects = Selection.objects;

            foreach (UnityEngine.Object go in gameObjects)
            {
                AssetImporter assetImporter = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(go.GetInstanceID()));

                assetImporter.assetBundleName = string.Empty;
            }

            AnalysisAssetBundleNames();

            RemoveUnusedAssetBundleNames();
        }

        // [MenuItem("Tools/AssetBundle/清理未使用的AB名")]
        public static void RemoveUnusedAssetBundleNames()
        {
            AssetDatabase.RemoveUnusedAssetBundleNames();
            AssetDatabase.Refresh();
        }

        // [MenuItem("Tools/AssetBundle/清理依赖的AB名")]
        public static void ClearAnalysisAssetBundleNames()
        {
            string[] abNames = AssetDatabase.GetAllAssetBundleNames();

            foreach (string abName in abNames)
            {
                if (string.IsNullOrWhiteSpace(abName))
                {
                    continue;
                }

                if (abName.StartsWith(dependentABPrefix))
                {
                    string[] abAssetPaths = AssetDatabase.GetAssetPathsFromAssetBundle(abName);

                    foreach (string assetPath in abAssetPaths)
                    {
                        if (IsCSharp(assetPath))
                        {
                            continue;
                        }

                        UnityEngine.Object go = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);

                        if (go)
                        {
                            AssetImporter assetImporter = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(go.GetInstanceID()));

                            assetImporter.assetBundleName = string.Empty;
                        }
                    }
                }
            }

            RemoveUnusedAssetBundleNames();
        }

        // [MenuItem("Tools/AssetBundle/重设依赖AB名(分析依赖自动设置AB名,并清理未使用的AB名)")]
        public static void AnalysisAssetBundleNames()
        {
            ClearAnalysisAssetBundleNames();

            string[] abNames = AssetDatabase.GetAllAssetBundleNames();

            var allABDependencies = new Dictionary<string, int>();

            foreach (string abName in abNames)
            {
                if (string.IsNullOrWhiteSpace(abName))
                {
                    continue;
                }

                string[] abAssetPaths = AssetDatabase.GetAssetPathsFromAssetBundle(abName);

                var abDependencies = new HashSet<string>();

                foreach (string abAssetPath in abAssetPaths)
                {
                    string[] abAssetDependencies = AssetDatabase.GetDependencies(abAssetPath);

                    foreach (string d in abAssetDependencies)
                    {
                        if (IsCSharp(d))
                        {
                            continue;
                        }

                        abDependencies.Add(d);
                    }
                }

                foreach (string item in abDependencies)
                {
                    if (allABDependencies.ContainsKey(item))
                    {
                        allABDependencies[item] += 1;
                    }
                    else
                    {
                        allABDependencies[item] = 1;
                    }
                }
            }

            foreach (var item in allABDependencies)
            {
                if (IsShader(item.Key))
                {
                    string[] abAssetDependencies = AssetDatabase.GetDependencies(item.Key);

                    foreach (string d in abAssetDependencies)
                    {
                        if (IsCSharp(d))
                        {
                            continue;
                        }

                        SetAssetABNameByAssetPath(d);
                    }

                    continue;
                }

                if (item.Value > 1)
                {
                    SetAssetABNameByAssetPath(item.Key);
                }
            }

            RemoveUnusedAssetBundleNames();
        }

        private static void SetAssetABNameByAssetPath(string assetPath)
        {
            UnityEngine.Object go = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);

            if (go)
            {
                string abName = $"{GetName(assetPath)}{unity3dEx}";

                AssetImporter assetImporter = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(go.GetInstanceID()));

                assetImporter.assetBundleName = abName;
            }
        }

        private static bool IsCSharp(string item)
        {
            return string.IsNullOrWhiteSpace(item) || item.EndsWith(".cs");
        }

        private static bool IsShader(string item)
        {
            return string.IsNullOrWhiteSpace(item) || item.EndsWith(".shader") || item.EndsWith(".compute") || item.EndsWith(".hlsl");
        }

        private static string GetName(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return string.Empty;
            }

            return key.Replace("/", "_").Replace(".", "_").Replace("-", "_").Replace(" ", "_").Trim().ToLower();
        }
    }
}