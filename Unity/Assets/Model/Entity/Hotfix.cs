using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
#if !ILRuntime
using System.Reflection;
#endif

namespace ETModel
{
	public sealed class Hotfix: Object
	{
#if ILRuntime
		private ILRuntime.Runtime.Enviorment.AppDomain appDomain;
		private MemoryStream dllStream;
		private MemoryStream pdbStream;
#else
		private Assembly assembly;
#endif

		private IStaticMethod start;
		private List<Type> hotfixTypes;

		public Action Update;
		public Action LateUpdate;
		public Action OnApplicationQuit;

		public void GotoHotfix()
		{
#if ILRuntime
			ILHelper.InitILRuntime(this.appDomain);
#endif
			this.start.Run();
		}

		public List<Type> GetHotfixTypes()
		{
			return this.hotfixTypes;
		}

		public void LoadHotfixAssembly()
		{
			Game.Scene.GetComponent<ResourcesComponent>().LoadBundle($"code.unity3d");
			GameObject code = (GameObject)Game.Scene.GetComponent<ResourcesComponent>().GetAsset("code.unity3d", "Code");
			
			byte[] assBytes = code.Get<TextAsset>("Hotfix.dll").bytes;
			byte[] pdbBytes = code.Get<TextAsset>("Hotfix.pdb").bytes;
			
#if ILRuntime
			Log.Debug($"当前使用的是ILRuntime模式");
			this.appDomain = new ILRuntime.Runtime.Enviorment.AppDomain();

			this.dllStream = new MemoryStream(assBytes);
			this.pdbStream = new MemoryStream(pdbBytes);
			this.appDomain.LoadAssembly(this.dllStream, this.pdbStream, new Mono.Cecil.Pdb.PdbReaderProvider());

			this.start = new ILStaticMethod(this.appDomain, "ETHotfix.Init", "Start", 0);
			
			this.hotfixTypes = this.appDomain.LoadedTypes.Values.Select(x => x.ReflectionType).ToList();
#else
			Log.Debug($"当前使用的是Mono模式");

			this.assembly = Assembly.Load(assBytes, pdbBytes);

			Type hotfixInit = this.assembly.GetType("ETHotfix.Init");
			this.start = new MonoStaticMethod(hotfixInit, "Start");
			
			this.hotfixTypes = this.assembly.GetTypes().ToList();
#endif
			
			Game.Scene.GetComponent<ResourcesComponent>().UnloadBundle($"code.unity3d");
		}
	}
}