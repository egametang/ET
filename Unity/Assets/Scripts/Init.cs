using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using ILRuntime.CLR.Method;
using ILRuntime.CLR.TypeSystem;
using ILRuntime.Runtime.Enviorment;
using MongoDB.Bson.IO;
using UnityEngine;

namespace Model
{
	public class Init : MonoBehaviour
	{
		public static Init Instance;

		public ILRuntime.Runtime.Enviorment.AppDomain AppDomain;

		private IStaticMethod start;
		private IStaticMethod update;
		private IStaticMethod lateUpdate;
		private IStaticMethod onApplicationQuit;

		private void Start()
		{
			try
			{
				if (Application.unityVersion != "2017.1.0p5")
				{
					Log.Error("请使用Unity2017.1.0p5版本");
				}

				DontDestroyOnLoad(gameObject);
				Instance = this;


				ObjectEvents.Instance.Add("Model", typeof(Init).Assembly);

#if ILRuntime
				Log.Debug("run in ilruntime mode");

				this.AppDomain = new ILRuntime.Runtime.Enviorment.AppDomain();
				DllHelper.LoadHotfixAssembly();
				ILHelper.InitILRuntime();
				
				this.start = new ILStaticMethod("Hotfix.Init", "Start", 0);
				this.update = new ILStaticMethod("Hotfix.Init", "Update", 0);
				this.lateUpdate = new ILStaticMethod("Hotfix.Init", "LateUpdate", 0);
				this.onApplicationQuit = new ILStaticMethod("Hotfix.Init", "OnApplicationQuit", 0);
#else
				Log.Debug("run in mono mode");
				ObjectEvents.Instance.HotfixAssembly = DllHelper.LoadHotfixAssembly();
				Type hotfixInit = ObjectEvents.Instance.HotfixAssembly.GetType("Hotfix.Init");
				this.start = new MonoStaticMethod(hotfixInit, "Start");
				this.update = new MonoStaticMethod(hotfixInit, "Update");
				this.lateUpdate = new MonoStaticMethod(hotfixInit, "LateUpdate");
				this.onApplicationQuit = new MonoStaticMethod(hotfixInit, "OnApplicationQuit");
#endif

				Game.Scene.AddComponent<OpcodeTypeComponent>();
				Game.Scene.AddComponent<MessageDispatherComponent>();
				Game.Scene.AddComponent<NetOuterComponent>();
				Game.Scene.AddComponent<ResourcesComponent>();
				Game.Scene.AddComponent<BehaviorTreeComponent>();
				Game.Scene.AddComponent<ConfigComponent>();

				// 进入热更新层
				this.start.Run();
			}
			catch (Exception e)
			{
				Log.Error(e.ToString());
			}
		}

		private void Update()
		{
			this.update.Run();
			ObjectEvents.Instance.Update();
		}

		private void LateUpdate()
		{
			this.lateUpdate.Run();
			ObjectEvents.Instance.LateUpdate();
		}

		private void OnApplicationQuit()
		{
			Instance = null;
			Game.Close();
			ObjectEvents.Close();
			this.onApplicationQuit.Run();
		}
	}
}