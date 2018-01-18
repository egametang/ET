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

		private async void Start()
		{
			try
			{
				if (Application.unityVersion != "2017.1.0p5")
				{
					Log.Error("请使用Unity2017.1.0p5版本");
				}

				DontDestroyOnLoad(gameObject);
				Instance = this;


				EventSystem.Instance.Add(DLLType.Model, typeof(Init).Assembly);

				Game.Scene.AddComponent<GlobalConfigComponent>();
				Game.Scene.AddComponent<OpcodeTypeComponent>();
				Game.Scene.AddComponent<NetOuterComponent>();
				Game.Scene.AddComponent<ResourcesComponent>();
				Game.Scene.AddComponent<BehaviorTreeComponent>();
				Game.Scene.AddComponent<PlayerComponent>();
				Game.Scene.AddComponent<UnitComponent>();
				Game.Scene.AddComponent<ClientFrameComponent>();
				Game.Scene.AddComponent<UIComponent>();


				// 下载ab包
				await BundleHelper.DownloadBundle();
				
				// 加载配置
				Game.Scene.GetComponent<ResourcesComponent>().LoadBundle("config.unity3d");
				Game.Scene.AddComponent<ConfigComponent>();
				Game.Scene.GetComponent<ResourcesComponent>().UnloadBundle("config.unity3d");
				
				Game.Scene.AddComponent<MessageDispatherComponent>();
#if ILRuntime
				Log.Debug("run in ilruntime mode");

				this.AppDomain = new ILRuntime.Runtime.Enviorment.AppDomain();
				Game.Scene.GetComponent<ResourcesComponent>().LoadBundle($"code.unity3d");
				ObjectEvents.Instance.LoadHotfixDll();
				Game.Scene.GetComponent<ResourcesComponent>().UnloadBundle($"code.unity3d");
				ILHelper.InitILRuntime();
				
				this.start = new ILStaticMethod("Hotfix.Init", "Start", 0);
				this.update = new ILStaticMethod("Hotfix.Init", "Update", 0);
				this.lateUpdate = new ILStaticMethod("Hotfix.Init", "LateUpdate", 0);
				this.onApplicationQuit = new ILStaticMethod("Hotfix.Init", "OnApplicationQuit", 0);
#else
				Log.Debug("run in mono mode");
				Game.Scene.GetComponent<ResourcesComponent>().LoadBundle($"code.unity3d");
				EventSystem.Instance.LoadHotfixDll();
				Game.Scene.GetComponent<ResourcesComponent>().UnloadBundle($"code.unity3d");
				Type hotfixInit = EventSystem.Instance.HotfixAssembly.GetType("Hotfix.Init");
				this.start = new MonoStaticMethod(hotfixInit, "Start");
				this.update = new MonoStaticMethod(hotfixInit, "Update");
				this.lateUpdate = new MonoStaticMethod(hotfixInit, "LateUpdate");
				this.onApplicationQuit = new MonoStaticMethod(hotfixInit, "OnApplicationQuit");
#endif

				// 进入热更新层
				this.start.Run();

				EventSystem.Instance.Run(EventIdType.InitSceneStart);
			}
			catch (Exception e)
			{
				Log.Error(e.ToString());
			}
		}

		private void Update()
		{
			this.update?.Run();
			EventSystem.Instance.Update();
		}

		private void LateUpdate()
		{
			this.lateUpdate?.Run();
			EventSystem.Instance.LateUpdate();
		}

		private void OnApplicationQuit()
		{
			Instance = null;
			Game.Close();
			EventSystem.Close();
			this.onApplicationQuit?.Run();
		}
	}
}