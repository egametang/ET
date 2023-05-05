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
		private Assembly model;

		public void Start()
		{
			if (Define.EnableCodes)
			{
				GlobalConfig globalConfig = Resources.Load<GlobalConfig>("GlobalConfig");
				if (globalConfig.CodeMode != CodeMode.ClientServer)
				{
					throw new Exception("ENABLE_CODES mode must use ClientServer code mode!");
				}
				
				Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
				Dictionary<string, Type> types = AssemblyHelper.GetAssemblyTypes(assemblies);
				EventSystem.Instance.Add(types);
				foreach (Assembly ass in assemblies)
				{
					string name = ass.GetName().Name;
					if (name == "Unity.Model.Codes")
					{
						this.model = ass;
					}
				}
			}
			else
			{
				byte[] assBytes;
				byte[] pdbBytes;
				if (!Define.IsEditor)
				{
					Dictionary<string, UnityEngine.Object> dictionary = AssetsBundleHelper.LoadBundle("code.unity3d");
					assBytes = ((TextAsset)dictionary["Model.dll"]).bytes;
					pdbBytes = ((TextAsset)dictionary["Model.pdb"]).bytes;

					if (Define.EnableIL2CPP)
					{
						HybridCLRHelper.Load();
					}
				}
				else
				{
					assBytes = File.ReadAllBytes(Path.Combine(Define.BuildOutputDir, "Model.dll"));
					pdbBytes = File.ReadAllBytes(Path.Combine(Define.BuildOutputDir, "Model.pdb"));
				}
			
				this.model = Assembly.Load(assBytes, pdbBytes);

				Dictionary<string, Type> types = AssemblyHelper.GetAssemblyTypes(typeof (Game).Assembly, typeof(Init).Assembly, this.model);
				EventSystem.Instance.Add(types);
			}
			IStaticMethod start = new StaticMethod(this.model, "ET.Entry", "Start");
			start.Run();
		}
	}
}