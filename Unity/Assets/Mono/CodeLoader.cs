#define ILRuntime1

using System;
using System.Collections.Generic;
using UnityEngine;

#if ILRuntime
using System.Linq;
#endif

namespace ET
{
	public class CodeLoader
	{
		public static CodeLoader Instance = new CodeLoader();
		
		public Action Update { get; set; }
		public Action LateUpdate { get; set; }
		public Action OnApplicationQuit { get; set; }

		private readonly IStaticMethod start;
		
		private readonly Type[] hotfixTypes;

		private CodeLoader()
		{
			Dictionary<string, UnityEngine.Object> dictionary = AssetsBundleHelper.LoadBundle("code.unity3d");
			byte[] assBytes = ((TextAsset)dictionary["Code.dll"]).bytes;
			byte[] pdbBytes = ((TextAsset)dictionary["Code.pdb"]).bytes;
			
#if ILRuntime
			ILRuntime.Runtime.Enviorment.AppDomain appDomain = new ILRuntime.Runtime.Enviorment.AppDomain();
			System.IO.MemoryStream assStream = new System.IO.MemoryStream(assBytes);
			System.IO.MemoryStream pdbStream = new System.IO.MemoryStream(pdbBytes);
			appDomain.LoadAssembly(assStream, pdbStream, new ILRuntime.Mono.Cecil.Pdb.PdbReaderProvider());
			
			ILHelper.InitILRuntime(appDomain);
			
			this.hotfixTypes = Type.EmptyTypes;
			this.hotfixTypes = appDomain.LoadedTypes.Values.Select(x => x.ReflectionType).ToArray();
			this.start = new ILStaticMethod(appDomain, "ET.Entry", "Start", 0);
#else

			System.Reflection.Assembly assembly = System.Reflection.Assembly.Load(assBytes, pdbBytes);
			hotfixTypes = assembly.GetTypes();
			this.start = new MonoStaticMethod(assembly, "ET.Entry", "Start");
#endif
		}
		
		public void Start()
		{
			this.start.Run();
		}

		public Type[] GetHotfixTypes()
		{
			return this.hotfixTypes;
		}
	}
}