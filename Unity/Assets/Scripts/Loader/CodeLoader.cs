using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using HybridCLR;
using UnityEngine;

namespace ET
{
    /// <summary>
    /// 代码加载器
    /// </summary>
    public class CodeLoader: Singleton<CodeLoader>, ISingletonAwake
    {
        /// <summary>
        /// model程序集
        /// </summary>
        private Assembly modelAssembly;
        /// <summary>
        /// modelView程序集
        /// </summary>
        private Assembly modelViewAssembly;

        /// <summary>
        /// Dll
        /// </summary>
        private Dictionary<string, TextAsset> dlls;
        /// <summary>
        /// 静态编译Dll
        /// </summary>
        private Dictionary<string, TextAsset> aotDlls;
        private bool enableDll;

        public void Awake()
        {
            //赋值是否启用Dll
            this.enableDll = Resources.Load<GlobalConfig>("GlobalConfig").EnableDll;
        }

        /// <summary>
        /// 加载Dll
        /// </summary>
        public async ETTask DownloadAsync()
        {
            //非编辑器下才需要加载Dll
            if (!Define.IsEditor)
            {
                //这里的API调用，路径参数只需要文件夹里的任一文件即可，具体底层会获取当前资源包文件夹里的全部文件
                this.dlls = await ResourcesComponent.Instance.LoadAllAssetsAsync<TextAsset>($"Assets/Bundles/Code/Unity.Model.dll.bytes");
                this.aotDlls = await ResourcesComponent.Instance.LoadAllAssetsAsync<TextAsset>($"Assets/Bundles/AotDlls/mscorlib.dll.bytes");
            }
        }

        /// <summary>
        /// 非Mono生命周期，在DownloadAsync后执行
        /// </summary>
        public void Start()
        {
            //如果不是编辑器，直接读取已经加载好的Dll
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

                //HybridCLR的IL2CPP的处理方式
                if (Define.EnableIL2CPP)
                {
                    foreach (var kv in this.aotDlls)
                    {
                        TextAsset textAsset = kv.Value;
                        //补充元数据，使用超集的模式（在允许使用裁剪后的Dll的情况下，还允许使用原始Dll进行补充）
                        RuntimeApi.LoadMetadataForAOTAssembly(textAsset.bytes, HomologousImageMode.SuperSet);
                    }
                }
                //加载model的程序集
                this.modelAssembly = Assembly.Load(modelAssBytes, modelPdbBytes);
                //加载modelView的程序集
                this.modelViewAssembly = Assembly.Load(modelViewAssBytes, modelViewPdbBytes);
            }
            else
            {
                //编辑器模式下，如果启动Dll的模式
                if (this.enableDll)
                {
                    //直接从文件读取后加载，无需补充元数据
                    byte[] modelAssBytes = File.ReadAllBytes(Path.Combine(Define.CodeDir, "Unity.Model.dll.bytes"));
                    byte[] modelPdbBytes = File.ReadAllBytes(Path.Combine(Define.CodeDir, "Unity.Model.pdb.bytes"));
                    byte[] modelViewAssBytes = File.ReadAllBytes(Path.Combine(Define.CodeDir, "Unity.ModelView.dll.bytes"));
                    byte[] modelViewPdbBytes = File.ReadAllBytes(Path.Combine(Define.CodeDir, "Unity.ModelView.pdb.bytes"));
                    this.modelAssembly = Assembly.Load(modelAssBytes, modelPdbBytes);
                    this.modelViewAssembly = Assembly.Load(modelViewAssBytes, modelViewPdbBytes);
                }
                else
                {
                    //反射加载
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
            
            //加载热更代码（元组？），与本方法类似
            (Assembly hotfixAssembly, Assembly hotfixViewAssembly) = this.LoadHotfix();
            
            //添加CodeTypes的单例并Awake
            World.Instance.AddSingleton<CodeTypes, Assembly[]>(new[]
            {
                //之前加载完毕的程序集
                typeof (World).Assembly, typeof (Init).Assembly, this.modelAssembly, this.modelViewAssembly, hotfixAssembly,hotfixViewAssembly
            });

            //热更代码结束，进入正式逻辑，反射执行ET.Entry类的Start方法
            IStaticMethod start = new StaticMethod(this.modelAssembly, "ET.Entry", "Start");
            start.Run();
        }

        /// <summary>
        /// 加载热更程序集
        /// </summary>
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