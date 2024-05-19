using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ET.Loader
{
    public class AssemblyDefinitionAsset
    {
        public string name;
        public string rootNamespace;
        public string[] references;
        public string[] includePlatforms;
        public string[] excludePlatforms;
        public bool allowUnsafeCode;
        public bool overrideReferences;
        public string[] precompiledReferences;
        public bool autoReferenced;
        public string[] defineConstraints;
        public string[] versionDefines;
        public bool noEngineReferences;
        public string[] optionalUnityReferences;
        public string[] additionalReferences;
        public string compilerOptions;
    }

    public class AllRefInfo
    {
        public Dictionary<string, HashSet<string>> References = new();
        
        public AllRefInfo()
        {
            foreach (string assName in AsmdefEditor.AssNames)
            {
                References[assName] = new HashSet<string>();
            }
        }
    }
    
    public static class AsmdefEditor
    {
        public static readonly string[] AssNames = { "Model", "ModelView", "Hotfix", "HotfixView" };
        public static readonly string[] PackagePaths = { "Packages", "Library/PackageCache" };
        
        // 自动把各个包中的引用加到Assets对应的包中去，后面搞个编辑器来编辑每个包的引用
        [MenuItem("ET/Update Assembly Definition")]
        public static void UpdateAssemblyDefinition()
        {
            AllRefInfo allRefInfo = new();

            foreach (var packagePath in PackagePaths)
            {
                foreach (string directory in Directory.GetDirectories(packagePath, "cn.etetet.*"))
                {
                    foreach (string assName in AssNames)
                    {
                        string p = Path.Combine(directory, "Scripts/" + assName + "/asmdef.txt");
                        if (!File.Exists(p))
                        {
                            continue;
                        }

                        string json = File.ReadAllText(p);
                        try
                        {
                            AssemblyDefinitionAsset assemblyDefinitionAsset = JsonUtility.FromJson<AssemblyDefinitionAsset>(json);
                            foreach (string reference in assemblyDefinitionAsset.references)
                            {
                                allRefInfo.References[assName].Add(reference);
                            }
                        }
                        catch (Exception e)
                        {
                            throw new Exception($"parse json error: {p} {json}", e);
                        }
                    }
                }
            }

            foreach (string assName in AssNames)
            {
                string p = Path.Combine("Assets/Scripts/" + assName + "/ET." + assName + ".asmdef");
                if (!File.Exists(p))
                {
                    continue;
                }

                string json = File.ReadAllText(p);
                AssemblyDefinitionAsset assemblyDefinitionAsset = JsonUtility.FromJson<AssemblyDefinitionAsset>(json);

                assemblyDefinitionAsset.references = allRefInfo.References[assName].ToArray();

                File.WriteAllText(p, JsonUtility.ToJson(assemblyDefinitionAsset, true));
            }
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}