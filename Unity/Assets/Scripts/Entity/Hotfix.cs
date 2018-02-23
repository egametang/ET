using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Model
{
	public sealed class Hotfix : Object
	{
		public Assembly HotfixAssembly;
		public ILRuntime.Runtime.Enviorment.AppDomain AppDomain;

		private readonly IStaticMethod start;

		public Action Update;
		public Action LateUpdate;
		public Action OnApplicationQuit;

		public Hotfix()
		{
			Game.Scene.GetComponent<ResourcesComponent>().LoadBundle($"code.unity3d");
			this.LoadHotfixAssembly();
			Game.Scene.GetComponent<ResourcesComponent>().UnloadBundle($"code.unity3d");

#if ILRuntime
			this.start = new ILStaticMethod(this.AppDomain, "Hotfix.Init", "Start", 0);
#else
			Type hotfixInit = this.HotfixAssembly.GetType("Hotfix.Init");
			this.start = new MonoStaticMethod(hotfixInit, "Start");
#endif
		}

		public void GotoHotfix()
		{
#if ILRuntime
			ILHelper.InitILRuntime();
#endif
			this.start.Run();
		}

		public Type[] GetHotfixTypes()
		{
#if ILRuntime
			if (this.AppDomain == null)
			{
				return new Type[0];
			}

			return this.AppDomain.LoadedTypes.Values.Select(x => x.ReflectionType).ToArray();
#else
			if (this.HotfixAssembly == null)
			{
				return new Type[0];
			}
			return this.HotfixAssembly.GetTypes();
#endif
		}


		public void LoadHotfixAssembly()
		{
#if ILRuntime
			this.AppDomain = new ILRuntime.Runtime.Enviorment.AppDomain();
			GameObject code = Game.Scene.GetComponent<ResourcesComponent>().GetAsset<GameObject>("code.unity3d", "Code");
			byte[] assBytes = code.Get<TextAsset>("Hotfix.dll").bytes;
			byte[] mdbBytes = code.Get<TextAsset>("Hotfix.pdb").bytes;

			using (MemoryStream fs = new MemoryStream(assBytes))
			using (MemoryStream p = new MemoryStream(mdbBytes))
			{
				this.AppDomain.LoadAssembly(fs, p, new Mono.Cecil.Pdb.PdbReaderProvider());
			}
#else
			GameObject code = Game.Scene.GetComponent<ResourcesComponent>().GetAsset<GameObject>("code.unity3d", "Code");
			byte[] assBytes = code.Get<TextAsset>("Hotfix.dll").bytes;
			byte[] mdbBytes = code.Get<TextAsset>("Hotfix.mdb").bytes;
			Assembly assembly = Assembly.Load(assBytes, mdbBytes);
			this.HotfixAssembly = assembly;
#endif
		}
	}
}