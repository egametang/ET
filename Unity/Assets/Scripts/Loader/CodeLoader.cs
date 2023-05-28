using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
#pragma warning disable CS0162

namespace ET
{
	public class CodeLoader: Singleton<CodeLoader>
	{
		private Assembly assembly;

		public void Start()
		{
			if (!Define.EnableCodes)
			{
				GlobalConfig globalConfig = Resources.Load<GlobalConfig>("GlobalConfig");
				if (globalConfig.CodeMode != CodeMode.ClientServer)
				{
					throw new Exception("!ENABLE_CODES mode must use ClientServer code mode!");
				}
				
				Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
				Dictionary<string, Type> types = AssemblyHelper.GetAssemblyTypes(assemblies);
				EventSystem.Instance.Add(types);
				foreach (Assembly ass in assemblies)
				{
					string name = ass.GetName().Name;
					if (name == "Unity.Model")
					{
						this.assembly = ass;
					}
				}
			}
			else
			{
				byte[] assBytes;
				byte[] pdbBytes;
				if (!Define.IsEditor)
				{
					//Dictionary<string, UnityEngine.Object> dictionary = AssetsBundleHelper.LoadBundle("code.unity3d");
					//assBytes = ((TextAsset)dictionary["Codes.dll"]).bytes;
					//pdbBytes = ((TextAsset)dictionary["Codes.pdb"]).bytes;
					
					// 这里为了方便做测试，直接加载了Unity/Temp/Bin/Debug/Codes.dll，真正打包要还原使用上面注释的代码
					assBytes = File.ReadAllBytes(Path.Combine("../Unity", Define.BuildOutputDir, "Codes.dll"));
					pdbBytes = File.ReadAllBytes(Path.Combine("../Unity", Define.BuildOutputDir, "Codes.pdb"));

					if (Define.EnableIL2CPP)
					{
						HybridCLRHelper.Load();
					}
				}
				else
				{
					assBytes = File.ReadAllBytes(Path.Combine(Define.BuildOutputDir, "Codes.dll"));
					pdbBytes = File.ReadAllBytes(Path.Combine(Define.BuildOutputDir, "Codes.pdb"));
				}
			
				this.assembly = Assembly.Load(assBytes, pdbBytes);

				Dictionary<string, Type> types = AssemblyHelper.GetAssemblyTypes(typeof (Game).Assembly, typeof(Init).Assembly, this.assembly);
				EventSystem.Instance.Add(types);
			}
			IStaticMethod start = new StaticMethod(this.assembly, "ET.Entry", "Start");
			start.Run();
		}
	}
}