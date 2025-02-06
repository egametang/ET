using System;
using System.IO;
using UnityEditor;

namespace ET
{
    public static class SceneNameSetHelper
    {
        public static void Run()
        {
            PackageGit packageGit = null;
            foreach (string directory in Directory.GetDirectories("./Packages"))
            {
                if (!File.Exists(Path.Combine(directory, "ET.sln")))
                {
                    continue;
                }

                packageGit = PackageGitHelper.Load(Path.Combine(directory, "packagegit.json"));
                break;
            }

            if (packageGit == null)
            {
                throw new Exception("not found demo et.sln");
            }
            
            GlobalConfig globalConfig = AssetDatabase.LoadAssetAtPath<GlobalConfig>("Packages/cn.etetet.loader/Resources/GlobalConfig.asset");
            globalConfig.SceneName = packageGit.Name;
            AssetDatabase.SaveAssets();
        }
    }
}