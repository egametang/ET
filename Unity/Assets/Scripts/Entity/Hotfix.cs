using System;
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

		public Type[] GetHotfixTypes()
		{
#if ILRuntime
			if (this.appDomain == null)
			{
				return new Type[0];
			}

			return this.appDomain.LoadedTypes.Values.Select(x => x.ReflectionType).ToArray();
#else
			if (this.assembly == null)
			{
				return new Type[0];
			}
			return this.assembly.GetTypes();
#endif
		}


		public void LoadHotfixAssembly()
		{
			Game.Scene.GetComponent<ResourcesComponent>().LoadBundle($"code.unity3d");
#if ILRuntime
			this.appDomain = new ILRuntime.Runtime.Enviorment.AppDomain();
			GameObject code = (GameObject)Game.Scene.GetComponent<ResourcesComponent>().GetAsset("code.unity3d", "Code");
			byte[] assBytes = code.Get<TextAsset>("Hotfix.dll").bytes;
			byte[] mdbBytes = code.Get<TextAsset>("Hotfix.pdb").bytes;

			using (MemoryStream fs = new MemoryStream(assBytes))
			using (MemoryStream p = new MemoryStream(mdbBytes))
			{
				this.appDomain.LoadAssembly(fs, p, new Mono.Cecil.Pdb.PdbReaderProvider());
			}

			this.start = new ILStaticMethod(this.appDomain, "ETHotfix.Init", "Start", 0);
#else
			GameObject code = (GameObject)Game.Scene.GetComponent<ResourcesComponent>().GetAsset("code.unity3d", "Code");
			byte[] assBytes = code.Get<TextAsset>("Hotfix.dll").bytes;
			byte[] mdbBytes = code.Get<TextAsset>("Hotfix.mdb").bytes;
			this.assembly = Assembly.Load(assBytes, mdbBytes);

			Type hotfixInit = this.assembly.GetType("ETHotfix.Init");
			this.start = new MonoStaticMethod(hotfixInit, "Start");
#endif
			Game.Scene.GetComponent<ResourcesComponent>().UnloadBundle($"code.unity3d");
		}
	}
}