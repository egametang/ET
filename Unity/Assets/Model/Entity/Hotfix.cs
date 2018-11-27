using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace ETModel
{
	public sealed class Hotfix : Object
	{
#if ILRuntime
		private ILRuntime.Runtime.Enviorment.AppDomain appDomain;
#else
		private Assembly assembly;
#endif

		private IStaticMethod start;

		public Action Update;
		public Action LateUpdate;
		public Action OnApplicationQuit;

		public Hotfix()
		{

		}

		public void GotoHotfix()
		{
#if ILRuntime
			ILHelper.InitILRuntime(this.appDomain);
#endif
			this.start.Run();
		}

		public List<Type> GetHotfixTypes()
		{
#if ILRuntime
			if (this.appDomain == null)
			{
				return new List<Type>();
			}

			return this.appDomain.LoadedTypes.Values.Select(x => x.ReflectionType).ToList();
#else
			if (this.assembly == null)
			{
				return new List<Type>();
			}
			return this.assembly.GetTypes().ToList();
#endif
		}


		public void LoadHotfixAssembly()
		{
			Game.Scene.GetComponent<ResourcesComponent>().LoadBundle($"code.unity3d");
			GameObject code = (GameObject)Game.Scene.GetComponent<ResourcesComponent>().GetAsset("code.unity3d", "Code");
			
#if ILRuntime
			Log.Debug($"当前使用的是ILRuntime模式");
			this.appDomain = new ILRuntime.Runtime.Enviorment.AppDomain();
			
			byte[] assBytes = code.Get<TextAsset>("Hotfix.dll").bytes;
			byte[] pdbBytes = code.Get<TextAsset>("Hotfix.pdb").bytes;

			using (MemoryStream fs = new MemoryStream(assBytes))
			using (MemoryStream p = new MemoryStream(pdbBytes))
			{
				this.appDomain.LoadAssembly(fs, p, new Mono.Cecil.Pdb.PdbReaderProvider());
			}

			this.start = new ILStaticMethod(this.appDomain, "ETHotfix.Init", "Start", 0);
#else
			Log.Debug($"当前使用的是Mono模式");
			byte[] assBytes = code.Get<TextAsset>("Hotfix.dll").bytes;
			byte[] pdbBytes = code.Get<TextAsset>("Hotfix.pdb").bytes;
			this.assembly = Assembly.Load(assBytes, pdbBytes);

			Type hotfixInit = this.assembly.GetType("ETHotfix.Init");
			this.start = new MonoStaticMethod(hotfixInit, "Start");
#endif
			Game.Scene.GetComponent<ResourcesComponent>().UnloadBundle($"code.unity3d");
		}
	}
}