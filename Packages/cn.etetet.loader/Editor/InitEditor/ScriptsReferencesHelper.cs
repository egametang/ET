using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ET
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
    
    public static class ScriptsReferencesHelper
    {
        public static readonly string[] AssNames = { "Model", "ModelView", "Hotfix", "HotfixView" };
        public static readonly string[] PackagePaths = { "Packages", "Library/PackageCache" };
        
        // 自动把各个包中的引用加到Assets对应的包中去，后面搞个编辑器来编辑每个包的引用
        [MenuItem("ET/Loader/UpdateScriptsReferences")]
        public static void Run()
        {
            Dictionary<string, HashSet<string>> refs = new ()
            {
                {"Model", new HashSet<string>()}, 
                {"Hotfix", new HashSet<string>()},
                {"ModelView", new HashSet<string>()}, 
                {"HotfixView", new HashSet<string>()}
            }; 
            foreach (string directory in Directory.GetDirectories("Packages", "cn.etetet.*"))
            {
                PackageGit packageGit = PackageGitHelper.Load(Path.Combine(directory, "packagegit.json"));
                if (packageGit.ScriptsReferences == null)
                {
                    continue;
                }
                foreach ((string assName, string[] references) in packageGit.ScriptsReferences)
                {
                    foreach (string s in references)
                    {
                        refs[assName].Add(s);
                    }
                }
            }

            string fourAssemblyDir = null;
            foreach (string directory in Directory.GetDirectories("Packages", "cn.etetet.*"))
            {
                if (File.Exists(Path.Combine(directory, "Runtime/Model/ET.Model.asmdef")))
                {
                    fourAssemblyDir = directory;
                    break;
                }
            }
            
            List<string> findRet = new List<string>();
            foreach ((string assName, HashSet<string> refAss) in refs)
            {
                findRet.Clear();
                FileHelper.GetAllFiles(findRet, fourAssemblyDir, $"ET.{assName}.asmdef");
                string p = findRet[0];
                if (!File.Exists(p))
                {
                    throw new Exception($"not found: {p}");
                }

                string json = File.ReadAllText(p);
                AssemblyDefinitionAsset assemblyDefinitionAsset = JsonUtility.FromJson<AssemblyDefinitionAsset>(json);

                List<string> list = refAss.ToList();
                list.Sort();
                assemblyDefinitionAsset.references = list.ToArray();

                File.WriteAllText(p, JsonUtility.ToJson(assemblyDefinitionAsset, true));
            }
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}