using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using HybridCLR;
using UnityEngine;

namespace ET
{
    public class CodeLoader: Singleton<CodeLoader>, ISingletonAwake
    {
        private Assembly modelAssembly;
        private Assembly modelViewAssembly;

        private Dictionary<string, TextAsset> dlls;
        private Dictionary<string, TextAsset> aotDlls;
        private bool enableDll;

        public void Awake()
        {
            this.enableDll = Resources.Load<GlobalConfig>("GlobalConfig").EnableDll;
        }

        public async ETTask DownloadAsync()
        {
            if (!Define.IsEditor)
            {
                this.dlls = await ResourcesComponent.Instance.LoadAllAssetsAsync<TextAsset>($"Assets/Bundles/Code/Unity.Model.dll.bytes");
                this.aotDlls = await ResourcesComponent.Instance.LoadAllAssetsAsync<TextAsset>($"Assets/Bundles/AotDlls/mscorlib.dll.bytes");
            }
        }

        public void Start()
        {
            if (!Define.IsEditor)
            {
                byte[] modelAssBytes = this.dlls["Unity.Model.dll"].bytes;
                byte[] modelPdbBytes = this.dlls["Unity.Model.pdb"].bytes;
                byte[] modelViewAssBytes = this.dlls["Unity.ModelView.dll"].bytes;
                byte[] modelViewPdbBytes = this.dlls["Unity.ModelView.pdb"].bytes;
                // 如果需要测试，可替换成下面注释的代码直接加载Assets/Bundles/Code/Unity.Model.dll.bytes，但真正打包时必须使用上面的代码
                //modelAssBytes = File.ReadAllBytes(Path.Combine(Define.CodeDir, "Unity.Model.dll.bytes"));
                //modelPdbBytes = File.ReadAllBytes(Path.Combine(Define.CodeDir, "Unity.Model.pdb.bytes"));
                //modelViewAssBytes = File.ReadAllBytes(Path.Combine(Define.CodeDir, "Unity.ModelView.dll.bytes"));
                //modelViewPdbBytes = File.ReadAllBytes(Path.Combine(Define.CodeDir, "Unity.ModelView.pdb.bytes"));

                if (Define.EnableIL2CPP)
                {
                    foreach (var kv in this.aotDlls)
                    {
                        TextAsset textAsset = kv.Value;
                        RuntimeApi.LoadMetadataForAOTAssembly(textAsset.bytes, HomologousImageMode.SuperSet);
                    }
                }
                this.modelAssembly = Assembly.Load(modelAssBytes, modelPdbBytes);
                this.modelViewAssembly = Assembly.Load(modelViewAssBytes, modelViewPdbBytes);
            }
            else
            {
                if (this.enableDll)
                {
                    byte[] modelAssBytes = File.ReadAllBytes(Path.Combine(Define.CodeDir, "Unity.Model.dll.bytes"));
                    byte[] modelPdbBytes = File.ReadAllBytes(Path.Combine(Define.CodeDir, "Unity.Model.pdb.bytes"));
                    byte[] modelViewAssBytes = File.ReadAllBytes(Path.Combine(Define.CodeDir, "Unity.ModelView.dll.bytes"));
                    byte[] modelViewPdbBytes = File.ReadAllBytes(Path.Combine(Define.CodeDir, "Unity.ModelView.pdb.bytes"));
                    this.modelAssembly = Assembly.Load(modelAssBytes, modelPdbBytes);
                    this.modelViewAssembly = Assembly.Load(modelViewAssBytes, modelViewPdbBytes);
                }
                else
                {
                    Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
                    foreach (Assembly ass in assemblies)
                    {
                        string name = ass.GetName().Name;
                        if (name == "Unity.Model")
                        {
                            this.modelAssembly = ass;
                        }
                        else if (name == "Unity.ModelView")
                        {
                            this.modelViewAssembly = ass;
                        }

                        if (this.modelAssembly != null && this.modelViewAssembly != null)
                        {
                            break;
                        }
                    }
                }
            }
            
            (Assembly hotfixAssembly, Assembly hotfixViewAssembly) = this.LoadHotfix();

            World.Instance.AddSingleton<CodeTypes, Assembly[]>(new[]
            {
                typeof (World).Assembly, typeof (Init).Assembly, this.modelAssembly, this.modelViewAssembly, hotfixAssembly,
                hotfixViewAssembly
            });

            IStaticMethod start = new StaticMethod(this.modelAssembly, "ET.Entry", "Start");
            start.Run();
        }

        private (Assembly, Assembly) LoadHotfix()
        {
            byte[] hotfixAssBytes;
            byte[] hotfixPdbBytes;
            byte[] hotfixViewAssBytes;
            byte[] hotfixViewPdbBytes;
            Assembly hotfixAssembly = null;
            Assembly hotfixViewAssembly = null;
            if (!Define.IsEditor)
            {
                hotfixAssBytes = this.dlls["Unity.Hotfix.dll"].bytes;
                hotfixPdbBytes = this.dlls["Unity.Hotfix.pdb"].bytes;
                hotfixViewAssBytes = this.dlls["Unity.HotfixView.dll"].bytes;
                hotfixViewPdbBytes = this.dlls["Unity.HotfixView.pdb"].bytes;
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
                if (this.enableDll)
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
            
            return (hotfixAssembly, hotfixViewAssembly);
        }

        public void Reload()
        {
            (Assembly hotfixAssembly, Assembly hotfixViewAssembly) = this.LoadHotfix();

            CodeTypes codeTypes = World.Instance.AddSingleton<CodeTypes, Assembly[]>(new[]
            {
                typeof (World).Assembly, typeof (Init).Assembly, this.modelAssembly, this.modelViewAssembly, hotfixAssembly,
                hotfixViewAssembly
            });
            codeTypes.CreateCode();

            Log.Info($"reload dll finish!");
        }
    }
}