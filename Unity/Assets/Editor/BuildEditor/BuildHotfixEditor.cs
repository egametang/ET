using System.Collections.Generic;
using System.IO;
using System.Linq;
using ETModel;
using UnityEditor;
using UnityEngine;

namespace ETEditor
{
    [InitializeOnLoad]
    public class Startup
    {
        //Unity代码生成dll位置
        private const string ScriptAssembliesDir = "Library/ScriptAssemblies";
        //热更代码dll文件
        private const string HotfixDll = "Unity.Hotfix.dll";
        //热更代码pdb文件
        private const string HotfixPdb = "Unity.Hotfix.pdb";
        //热更代码存放位置
        private const string CodeDir = "Assets/Res/Code/";
        //配置存放位置
        private const string ConfigDir = "Assets/Res/Config/";
        //ET内置prefab存放位置
        private const string BundlesDir = "Assets/Bundles/Independent/";

        static Startup()
        {
            //拷贝热更代码
            CopyCode();
            //检查Code.prefab
            CheckCode();
            //检查Config.prefab
            CheckConfig();
        }
        static void CopyCode()
        {
            Log.Info($"Copy Hotfix Code");
            if (!Directory.Exists(CodeDir))
                Directory.CreateDirectory(CodeDir);
            File.Copy(Path.Combine(ScriptAssembliesDir, HotfixDll), Path.Combine(CodeDir, "Hotfix.dll.bytes"), true);
            File.Copy(Path.Combine(ScriptAssembliesDir, HotfixPdb), Path.Combine(CodeDir, "Hotfix.pdb.bytes"), true);
            Log.Info($"复制Hotfix.dll, Hotfix.pdb到Res/Code完成");
            AssetDatabase.Refresh();
        }
        /// <summary>
        /// 校验Code.prefab
        /// </summary>
        static void CheckCode()
        {
            Log.Info($"Check Code.prefab");
            GameObject prefab = LoadIndependent("Code");
            ReferenceCollector rc = prefab.GetComponent<ReferenceCollector>();
            //检查Code.prefab的空引用
            bool CheckNull(string type)
            {
                UnityEngine.Object obj = prefab.Get<UnityEngine.Object>("Hotfix."+ type);
                if (obj == null)
                {
                    string objPath = Path.Combine(CodeDir, $"Hotfix.{type}.bytes");
                    obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(objPath);
                    if (obj == null)
                        Log.Error($"{objPath} Not Find");
                    rc.Add("Hotfix." + type, obj);
                    Log.Warning($"Code.prefab 自动添加 Hotfix.{type}");
                    return true;
                }
                return false;
            }
            bool checkDll = CheckNull("dll");
            bool checkPdb = CheckNull("pdb");
            if (checkDll || checkPdb)
            {
                Log.Debug($"自动更新 Code.prefab");
                EditorUtility.SetDirty(prefab);
                AssetDatabase.Refresh();
            }
        }
        /// <summary>
        /// 校验Config.prefab
        /// </summary>
        static void CheckConfig()
        {
            Log.Info($"Check Config.prefab");
            GameObject prefab = LoadIndependent("Config");
            bool hasConfig = Directory.Exists(ConfigDir);
            List<string> configFiles = new List<string>();
            if (hasConfig)
                configFiles = Directory.GetFiles(ConfigDir, "*.txt")?.ToList();
            if (!hasConfig || configFiles.Count == 0)
            {
                EditorApplication.isPlaying = false;
                EditorUtility.DisplayDialog("提示", "当前还没有生成配置表,请先生成配置表", "确定");
                return;
            }
            ReferenceCollector rc = prefab.GetComponent<ReferenceCollector>();
            bool CheckNull(string configFile)
            {
                string fileName = Path.GetFileNameWithoutExtension(configFile);
                UnityEngine.Object obj = prefab.Get<UnityEngine.Object>(fileName);
                if (obj == null)
                {
                    obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(configFile);
                    rc.Add(fileName, obj);
                    Log.Warning($"Config.prefab 自动添加 {fileName}");
                    return true;
                }
                return false;
            }
            bool isUpdate = false;
            foreach (var configFile in configFiles)
                if (CheckNull(configFile))
                    isUpdate = true;
            if (isUpdate)
            {
                Log.Debug($"自动更新 Config.prefab");
                EditorUtility.SetDirty(prefab);
                AssetDatabase.Refresh();
            }
        }
        /// <summary>
        /// 加载ET内置prefab
        /// </summary>
        static GameObject LoadIndependent(string prefabName)
        {
            if (!Directory.Exists(BundlesDir))
                Directory.CreateDirectory(BundlesDir);
            string path = $"{BundlesDir}{prefabName}.prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null)
            {
                Log.Warning($"{path} 不存在 自动创建{prefabName}.prefab");
                prefab = new GameObject(prefabName);
                prefab.AddComponent<ReferenceCollector>();
                PrefabUtility.SaveAsPrefabAsset(prefab, path, out bool sucess);
                AssetDatabase.Refresh();
                UnityEngine.Object.DestroyImmediate(prefab);
            }
            //设置Code的AB名字
            var importer = AssetImporter.GetAtPath(path);
            string abName = $"{prefabName.ToLower()}.unity3d";
            if (importer.assetBundleName != abName)
            {
                importer.assetBundleName = $"{prefabName.ToLower()}.unity3d";
                AssetDatabase.Refresh();
                prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            }
            //不选中修改的RC数据保存不下去
            EditorGUIUtility.PingObject(prefab);
            Selection.activeObject = prefab;
            return prefab;
        }
    }
}
