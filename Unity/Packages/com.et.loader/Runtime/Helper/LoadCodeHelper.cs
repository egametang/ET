using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using HybridCLR;
using UnityEngine;

namespace ET
{
    public static class LoadCodeHelper
    {
        public static async ETTask LoadDlls()
        {
            var dlls = await DownloadAsync();
            
            LoadModel(dlls);
            LoadHotfix(dlls);

            if (Define.EnableIL2CPP)
            {
                await LoadAot();
            }
        }
        
        public static async ETTask ReLoadDlls()
        {
            var dlls = await DownloadAsync();
            LoadHotfix(dlls);
        }

        private static async ETTask<Dictionary<string, TextAsset>> DownloadAsync()
        {
            return await ResourcesComponent.Instance.LoadAllAssetsAsync<TextAsset>($"Assets/Bundles/Code/Unity.Model.dll.bytes");
        }

        private static async ETTask LoadAot()
        {
            Dictionary<string, TextAsset> aotDlls = await ResourcesComponent.Instance.LoadAllAssetsAsync<TextAsset>($"Assets/Bundles/AotDlls/mscorlib.dll.bytes");
            foreach (var kv in aotDlls)
            {
                TextAsset textAsset = kv.Value;
                RuntimeApi.LoadMetadataForAOTAssembly(textAsset.bytes, HomologousImageMode.SuperSet);
            }
        }

        private static void LoadModel(Dictionary<string, TextAsset> dlls)
        {
            bool enableDll = Resources.Load<GlobalConfig>("GlobalConfig").EnableDll;
            Assembly modelAssembly = null;
            Assembly modelViewAssembly = null;
            
            if (!Define.IsEditor)
            {
                byte[] modelAssBytes = dlls["Unity.Model.dll"].bytes;
                byte[] modelPdbBytes = dlls["Unity.Model.pdb"].bytes;
                byte[] modelViewAssBytes = dlls["Unity.ModelView.dll"].bytes;
                byte[] modelViewPdbBytes = dlls["Unity.ModelView.pdb"].bytes;
                // 如果需要测试，可替换成下面注释的代码直接加载Assets/Bundles/Code/Unity.Model.dll.bytes，但真正打包时必须使用上面的代码
                //modelAssBytes = File.ReadAllBytes(Path.Combine(Define.CodeDir, "Unity.Model.dll.bytes"));
                //modelPdbBytes = File.ReadAllBytes(Path.Combine(Define.CodeDir, "Unity.Model.pdb.bytes"));
                //modelViewAssBytes = File.ReadAllBytes(Path.Combine(Define.CodeDir, "Unity.ModelView.dll.bytes"));
                //modelViewPdbBytes = File.ReadAllBytes(Path.Combine(Define.CodeDir, "Unity.ModelView.pdb.bytes"));

                
                modelAssembly = Assembly.Load(modelAssBytes, modelPdbBytes);
                modelViewAssembly = Assembly.Load(modelViewAssBytes, modelViewPdbBytes);
            }
            else
            {
                if (enableDll)
                {
                    byte[] modelAssBytes = File.ReadAllBytes(Path.Combine(Define.CodeDir, "Unity.Model.dll.bytes"));
                    byte[] modelPdbBytes = File.ReadAllBytes(Path.Combine(Define.CodeDir, "Unity.Model.pdb.bytes"));
                    byte[] modelViewAssBytes = File.ReadAllBytes(Path.Combine(Define.CodeDir, "Unity.ModelView.dll.bytes"));
                    byte[] modelViewPdbBytes = File.ReadAllBytes(Path.Combine(Define.CodeDir, "Unity.ModelView.pdb.bytes"));
                    modelAssembly = Assembly.Load(modelAssBytes, modelPdbBytes);
                    modelViewAssembly = Assembly.Load(modelViewAssBytes, modelViewPdbBytes);
                }
                else
                {
                    Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
                    foreach (Assembly ass in assemblies)
                    {
                        string name = ass.GetName().Name;
                        if (name == "Unity.Model")
                        {
                            modelAssembly = ass;
                        }
                        else if (name == "Unity.ModelView")
                        {
                            modelViewAssembly = ass;
                        }

                        if (modelAssembly != null && modelViewAssembly != null)
                        {
                            break;
                        }
                    }
                }
            }
            
            CodeLoader.Instance.AddModel(typeof(World).Assembly, typeof(Init).Assembly, modelAssembly, modelViewAssembly);
        }

        private static void LoadHotfix(Dictionary<string, TextAsset> dlls)
        {
            bool enableDll = Resources.Load<GlobalConfig>("GlobalConfig").EnableDll;
            byte[] hotfixAssBytes;
            byte[] hotfixPdbBytes;
            byte[] hotfixViewAssBytes;
            byte[] hotfixViewPdbBytes;
            Assembly hotfixAssembly = null;
            Assembly hotfixViewAssembly = null;
            if (!Define.IsEditor)
            {
                hotfixAssBytes = dlls["Unity.Hotfix.dll"].bytes;
                hotfixPdbBytes = dlls["Unity.Hotfix.pdb"].bytes;
                hotfixViewAssBytes = dlls["Unity.HotfixView.dll"].bytes;
                hotfixViewPdbBytes = dlls["Unity.HotfixView.pdb"].bytes;
                // 如果需要测试，可替换成下面注释的代码直接加载Assets/Bundles/Code/Hotfix.dll.bytes，但真正打包时必须使用上面的代码
                //hotfixAssBytes = File.ReadAllBytes(Path.Combine(Define.CodeDir, "Unity.Hotfix.dll.bytes"));
                //hotfixPdbBytes = File.ReadAllBytes(Path.Combine(Define.CodeDir, "Unity.Hotfix.pdb.bytes"));
                //hotfixViewAssBytes = File.ReadAllBytes(Path.Combine(Define.CodeDir, "Unity.HotfixView.dll.bytes"));
                //hotfixViewPdbBytes = File.ReadAllBytes(Path.Combine(Define.CodeDir, "Unity.HotfixView.pdb.bytes"));
                hotfixAssembly = Assembly.Load(hotfixAssBytes, hotfixPdbBytes);
                hotfixViewAssembly = Assembly.Load(hotfixViewAssBytes, hotfixViewPdbBytes);
            }
            else
            {
                if (enableDll)
                {
                    hotfixAssBytes = File.ReadAllBytes(Path.Combine(Define.CodeDir, "Unity.Hotfix.dll.bytes"));
                    hotfixPdbBytes = File.ReadAllBytes(Path.Combine(Define.CodeDir, "Unity.Hotfix.pdb.bytes"));
                    hotfixViewAssBytes = File.ReadAllBytes(Path.Combine(Define.CodeDir, "Unity.HotfixView.dll.bytes"));
                    hotfixViewPdbBytes = File.ReadAllBytes(Path.Combine(Define.CodeDir, "Unity.HotfixView.pdb.bytes"));
                    hotfixAssembly = Assembly.Load(hotfixAssBytes, hotfixPdbBytes);
                    hotfixViewAssembly = Assembly.Load(hotfixViewAssBytes, hotfixViewPdbBytes);
                }
                else
                {
                    Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
                    foreach (Assembly ass in assemblies)
                    {
                        string name = ass.GetName().Name;
                        if (name == "Unity.Hotfix")
                        {
                            hotfixAssembly = ass;
                        }
                        else if (name == "Unity.HotfixView")
                        {
                            hotfixViewAssembly = ass;
                        }

                        if (hotfixAssembly != null && hotfixViewAssembly != null)
                        {
                            break;
                        }
                    }
                }
            }

            CodeLoader.Instance.LoadHotfix(hotfixAssembly, hotfixViewAssembly);
        }
    }
}