#define ILRuntime

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using System.Linq;
using AppDomain = ILRuntime.Runtime.Enviorment.AppDomain;

namespace ET
{
	public class CodeLoader
	{
		public static CodeLoader Instance = new CodeLoader();

		public Action Update;
		public Action LateUpdate;
		public Action OnApplicationQuit;

		private Assembly assembly;
		
		private Type[] allTypes;

		private CodeLoader()
		{
		}
		
		public void Start()
		{
			switch (Define.CodeMode)
			{
				case Define.CodeMode_Mono:
				{
					Dictionary<string, UnityEngine.Object> dictionary = AssetsBundleHelper.LoadBundle("code.unity3d");
					byte[] assBytes = ((TextAsset)dictionary["Code.dll"]).bytes;
					byte[] pdbBytes = ((TextAsset)dictionary["Code.pdb"]).bytes;
					
					assembly = Assembly.Load(assBytes, pdbBytes);
					this.allTypes = assembly.GetTypes();
					IStaticMethod start = new MonoStaticMethod(assembly, "ET.Entry", "Start");
					start.Run();
					break;
				}
				case Define.CodeMode_ILRuntime:
				{
					Dictionary<string, UnityEngine.Object> dictionary = AssetsBundleHelper.LoadBundle("code.unity3d");
					byte[] assBytes = ((TextAsset)dictionary["Code.dll"]).bytes;
					byte[] pdbBytes = ((TextAsset)dictionary["Code.pdb"]).bytes;
				
					AppDomain appDomain = new AppDomain();
					MemoryStream assStream = new MemoryStream(assBytes);
					MemoryStream pdbStream = new MemoryStream(pdbBytes);
					appDomain.LoadAssembly(assStream, pdbStream, new ILRuntime.Mono.Cecil.Pdb.PdbReaderProvider());

					ILHelper.InitILRuntime(appDomain);

					this.allTypes = appDomain.LoadedTypes.Values.Select(x => x.ReflectionType).ToArray();
					IStaticMethod start = new ILStaticMethod(appDomain, "ET.Entry", "Start", 0);
					start.Run();
					break;
				}
				case Define.CodeMode_Reload:
				{
					byte[] assBytes = File.ReadAllBytes(Path.Combine(Define.BuildOutputDir, "Data.dll"));
					byte[] pdbBytes = File.ReadAllBytes(Path.Combine(Define.BuildOutputDir, "Data.pdb"));
					
					assembly = Assembly.Load(assBytes, pdbBytes);
					LoadHotfix();
					IStaticMethod start = new MonoStaticMethod(assembly, "ET.Entry", "Start");
					start.Run();
					break;
				}
			}
		}

		// 热重载调用下面三个方法
		// CodeLoader.Instance.LoadHotfix();
		// Game.EventSystem.Add(CodeLoader.Instance.GetTypes());
		// Game.EventSystem.Load();
		public void LoadHotfix()
		{
			// 傻屌Unity在这里搞了个傻逼优化，认为同一个路径的dll，返回的程序集就一样。所以这里每次编译都要随机名字
			string[] logicFiles = Directory.GetFiles(Define.BuildOutputDir, "Logic_*.dll");
			if (logicFiles.Length != 1)
			{
				throw new Exception("Logic dll count != 1");
			}

			string logicName = Path.GetFileNameWithoutExtension(logicFiles[0]);
			byte[] assBytes = File.ReadAllBytes(Path.Combine(Define.BuildOutputDir, $"{logicName}.dll"));
			byte[] pdbBytes = File.ReadAllBytes(Path.Combine(Define.BuildOutputDir, $"{logicName}.pdb"));

			Assembly hotfixAssembly = Assembly.Load(assBytes, pdbBytes);
			
			List<Type> listType = new List<Type>();
			listType.AddRange(this.assembly.GetTypes());
			listType.AddRange(hotfixAssembly.GetTypes());
			this.allTypes = listType.ToArray();
		}

		public Type[] GetTypes()
		{
			return this.allTypes;
		}
	}
}