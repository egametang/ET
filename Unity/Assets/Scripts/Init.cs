using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using ILRuntime.CLR.Method;
using ILRuntime.CLR.TypeSystem;
using ILRuntime.Runtime.Enviorment;
using UnityEngine;

namespace Model
{
	public class Init : MonoBehaviour
	{
		public static Init Instance;

		public ILRuntime.Runtime.Enviorment.AppDomain AppDomain;

		private IStaticMethod start;

		public Action HotfixUpdate;
		public Action HotfixLateUpdate;
		public Action HotfixOnApplicationQuit;

		private async void Start()
		{
			try
			{
				if (Application.unityVersion != "2017.1.0p5")
				{
					Log.Warning($"当前版本:{Application.unityVersion}, 最好使用运行指南推荐版本!");
				}

				DontDestroyOnLoad(gameObject);
				Instance = this;


				Game.EventSystem.Add(DLLType.Model, typeof(Init).Assembly);

				Game.Scene.AddComponent<GlobalConfigComponent>();
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
				Game.EventSystem.LoadHotfixDll();
				Game.Scene.GetComponent<ResourcesComponent>().UnloadBundle($"code.unity3d");
				ILHelper.InitILRuntime();
				
				this.start = new ILStaticMethod("Hotfix.Init", "Start", 0);
#else
				Log.Debug("run in mono mode");
				Game.Scene.GetComponent<ResourcesComponent>().LoadBundle($"code.unity3d");
				Game.EventSystem.LoadHotfixDll();
				Game.Scene.GetComponent<ResourcesComponent>().UnloadBundle($"code.unity3d");
				Type hotfixInit = Game.EventSystem.HotfixAssembly.GetType("Hotfix.Init");
				this.start = new MonoStaticMethod(hotfixInit, "Start");
#endif

				Game.Scene.AddComponent<OpcodeTypeComponent>();

				// 进入热更新层
				this.start.Run();

				Game.EventSystem.Run(EventIdType.TestHotfixSubscribMonoEvent, "TestHotfixSubscribMonoEvent");
			}
			catch (Exception e)
			{
				Log.Error(e.ToString());
			}
		}

		private void Update()
		{
			this.HotfixUpdate.Invoke();
			Game.EventSystem.Update();
		}

		private void LateUpdate()
		{
			this.HotfixLateUpdate.Invoke();
			Game.EventSystem.LateUpdate();
		}

		private void OnApplicationQuit()
		{
			Instance = null;
			Game.Close();
			this.HotfixOnApplicationQuit.Invoke();
		}
	}
}