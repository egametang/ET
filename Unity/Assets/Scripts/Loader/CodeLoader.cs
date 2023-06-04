using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace ET
{
    /// <summary>
    /// 一个加载和运行代码程序集的单例类。
    /// </summary>
    public class CodeLoader: Singleton<CodeLoader>
	{
        // 模型程序集的引用
        private Assembly model;

		public void Start()
		{
			if (Define.EnableCodes)
			{
                // 如果启用了代码模式
                // 加载全局配置文件
                GlobalConfig globalConfig = Resources.Load<GlobalConfig>("GlobalConfig");
				if (globalConfig.CodeMode != CodeMode.ClientServer)
				{
					throw new Exception("ENABLE_CODES mode must use ClientServer code mode!");
				}

                // 获取当前域中的所有程序集
                Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

                // 获取所有程序集中的所有类型
                Dictionary<string, Type> types = AssemblyHelper.GetAssemblyTypes(assemblies);

                // 将类型添加到事件系统中
                EventSystem.Instance.Add(types);

                // 遍历所有程序集
                foreach (Assembly ass in assemblies)
				{
                    // 获取程序集的名称
                    string name = ass.GetName().Name;
					if (name == "Unity.Model.Codes")
					{
                        // 如果程序集的名称是Unity.Model.Codes
                        // 将该程序集赋值给model字段
                        this.model = ass;
					}
				}
			}
			else
			{
				// 如果没有启用代码模式
				byte[] assBytes; // 用于存储模型程序集的字节数组
                byte[] pdbBytes; // 用于存储模型程序集的符号文件的字节数组
                if (!Define.IsEditor)
				{
                    // 如果不是在编辑器中运行

                    // 加载code.unity3d资源包
                    Dictionary<string, UnityEngine.Object> dictionary = AssetsBundleHelper.LoadBundle("code.unity3d");

                    // 从资源包中获取Model.dll文件的字节内容
                    assBytes = ((TextAsset)dictionary["Model.dll"]).bytes;

                    // 从资源包中获取Model.pdb文件的字节内容
                    pdbBytes = ((TextAsset)dictionary["Model.pdb"]).bytes;

					if (Define.EnableIL2CPP)
					{
                        // 如果启用了IL2CPP模式
                        HybridCLRHelper.Load();
					}
				}
				else
				{
                    // 如果是在编辑器中运行

                    // 从BuildOutputDir目录下读取Model.dll文件的字节内容
                    assBytes = File.ReadAllBytes(Path.Combine(Define.BuildOutputDir, "Model.dll"));

                    // 从BuildOutputDir目录下读取Model.pdb文件的字节内容
                    pdbBytes = File.ReadAllBytes(Path.Combine(Define.BuildOutputDir, "Model.pdb"));
				}

                // 使用字节内容加载模型程序集并赋值给model字段
                this.model = Assembly.Load(assBytes, pdbBytes);

                // 调用LoadHotfix方法加载热重载程序集
                this.LoadHotfix();
			}

            // 创建一个静态方法对象，表示模型程序集中ET.Entry类的Start方法
            IStaticMethod start = new StaticMethod(this.model, "ET.Entry", "Start");

            // 运行该静态方法，作为代码入口点
            start.Run();
		}

		// 热重载调用该方法
		public void LoadHotfix()
		{
			byte[] assBytes;  // 用于存储热重载程序集的字节数组
            byte[] pdbBytes;  // 用于存储热重载程序集的符号文件的字节数组
            if (!Define.IsEditor)
			{

                // 如果不是在编辑器中运行
                // 加载code.unity3d资源包
                Dictionary<string, UnityEngine.Object> dictionary = AssetsBundleHelper.LoadBundle("code.unity3d");

                // 从资源包中获取Hotfix.dll文件的字节内容
                assBytes = ((TextAsset)dictionary["Hotfix.dll"]).bytes;

                // 从资源包中获取Hotfix.pdb文件的字节内容
                pdbBytes = ((TextAsset)dictionary["Hotfix.pdb"]).bytes;
			}
			else
			{
                // 如果是在编辑器中运行

                // 傻屌Unity在这里搞了个傻逼优化，认为同一个路径的dll，返回的程序集就一样。所以这里每次编译都要随机名字
                string[] logicFiles = Directory.GetFiles(Define.BuildOutputDir, "Hotfix_*.dll");
				if (logicFiles.Length != 1)
                {
                    // 如果dll文件数量不等于1
                    throw new Exception("Logic dll count != 1");
				}

                // 获取dll文件的名称（不含扩展名）
                string logicName = Path.GetFileNameWithoutExtension(logicFiles[0]);

                // 从BuildOutputDir目录下读取dll文件的字节内容
                assBytes = File.ReadAllBytes(Path.Combine(Define.BuildOutputDir, $"{logicName}.dll"));

                // 从BuildOutputDir目录下读取pdb文件的字节内容
                pdbBytes = File.ReadAllBytes(Path.Combine(Define.BuildOutputDir, $"{logicName}.pdb"));
			}

            // 使用字节内容加载热重载程序集
            Assembly hotfixAssembly = Assembly.Load(assBytes, pdbBytes);

            // 获取所有相关程序集中的所有类型
            Dictionary<string, Type> types = AssemblyHelper.GetAssemblyTypes(typeof (Game).Assembly, typeof(Init).Assembly, this.model, hotfixAssembly);

            // 将类型添加到事件系统中
            EventSystem.Instance.Add(types);
		}
	}
}