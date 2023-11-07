using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using HybridCLR;
using UnityEngine;

namespace ET
{
    public class CodeLoader : Singleton<CodeLoader>, ISingletonAwake
    {
        private Assembly assembly;

        private Dictionary<string, TextAsset> dlls;
        private Dictionary<string, TextAsset> aotDlls;

        public void Awake()
        {
        }

        public async ETTask DownloadAsync()
        {
            if (!Define.IsEditor)
            {
                this.dlls = await ResourcesComponent.Instance.LoadAllAssetsAsync<TextAsset>($"Assets/Bundles/Code/Model.dll.bytes");
                this.aotDlls = await ResourcesComponent.Instance.LoadAllAssetsAsync<TextAsset>($"Assets/Bundles/AotDlls/mscorlib.dll.bytes");
            }
        }

        public void Start()
        {
            if (!Define.EnableDll)
            {
                GlobalConfig globalConfig = Resources.Load<GlobalConfig>("GlobalConfig");
                if (globalConfig.CodeMode != CodeMode.ClientServer)
                {
                    throw new Exception("!ENABLE_DLL mode must use ClientServer code mode!");
                }

                Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

                foreach (Assembly ass in assemblies)
                {
                    string name = ass.GetName().Name;
                    if (name == "Unity.Model")
                    {
                        this.assembly = ass;
                        break;
                    }
                }

                World.Instance.AddSingleton<CodeTypes, Assembly[]>(assemblies);
            }
            else
            {
                byte[] assBytes;
                byte[] pdbBytes;
                if (!Define.IsEditor)
                {
                    assBytes = this.dlls["Model.dll"].bytes;
                    pdbBytes = this.dlls["Model.pdb"].bytes;

                    // 如果需要测试，可替换成下面注释的代码直接加载Assets/Bundles/Code/Model.dll.bytes，但真正打包时必须使用上面的代码
                    //assBytes = File.ReadAllBytes(Path.Combine(Define.CodeDir, "Model.dll.bytes"));
                    //pdbBytes = File.ReadAllBytes(Path.Combine(Define.CodeDir, "Model.pdb.bytes"));

                    if (Define.EnableIL2CPP)
                    {
                        foreach (var kv in this.aotDlls)
                        {
                            TextAsset textAsset = kv.Value;
                            RuntimeApi.LoadMetadataForAOTAssembly(textAsset.bytes, HomologousImageMode.SuperSet);
                        }
                    }
                }
                else
                {
                    assBytes = File.ReadAllBytes(Path.Combine(Define.CodeDir, "Model.dll.bytes"));
                    pdbBytes = File.ReadAllBytes(Path.Combine(Define.CodeDir, "Model.pdb.bytes"));
                }

                this.assembly = Assembly.Load(assBytes, pdbBytes);

                Assembly hotfixAssembly = this.LoadHotfix();

                World.Instance.AddSingleton<CodeTypes, Assembly[]>(new[] { typeof(World).Assembly, typeof(Init).Assembly, this.assembly, hotfixAssembly });
            }

            IStaticMethod start = new StaticMethod(this.assembly, "ET.Entry", "Start");
            start.Run();
        }

        private Assembly LoadHotfix()
        {
            byte[] assBytes;
            byte[] pdbBytes;
            if (!Define.IsEditor)
            {
                assBytes = this.dlls["Hotfix.dll"].bytes;
                pdbBytes = this.dlls["Hotfix.pdb"].bytes;

                // 如果需要测试，可替换成下面注释的代码直接加载Assets/Bundles/Code/Hotfix.dll.bytes，但真正打包时必须使用上面的代码
                //assBytes = File.ReadAllBytes(Path.Combine(Define.CodeDir, "Hotfix.dll.bytes"));
                //pdbBytes = File.ReadAllBytes(Path.Combine(Define.CodeDir, "Hotfix.pdb.bytes"));
            }
            else
            {
                assBytes = File.ReadAllBytes(Path.Combine(Define.CodeDir, "Hotfix.dll.bytes"));
                pdbBytes = File.ReadAllBytes(Path.Combine(Define.CodeDir, "Hotfix.pdb.bytes"));
            }

            Assembly hotfixAssembly = Assembly.Load(assBytes, pdbBytes);
            return hotfixAssembly;
        }

        public void Reload()
        {
            Assembly hotfixAssembly = this.LoadHotfix();

            CodeTypes codeTypes = World.Instance.AddSingleton<CodeTypes, Assembly[]>(new[] { typeof(World).Assembly, typeof(Init).Assembly, this.assembly, hotfixAssembly });
            codeTypes.CreateCode();

            Log.Debug($"reload dll finish!");
        }
    }
}