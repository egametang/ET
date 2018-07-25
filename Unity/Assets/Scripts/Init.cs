using System;
using System.IO;
using System.Threading;
using Google.Protobuf;
using UnityEngine;

namespace ETModel
{
	public class Init : MonoBehaviour
	{
		private async void Start()
		{
			try
			{
				if (Application.unityVersion != "2017.4.3f1")
				{
					Log.Error($"新人请使用Unity2017.4.3f1,减少跑demo遇到的问题! 下载地址:\n https://download.unity3d.com/download_unity/21ae32b5a9cb/UnityDownloadAssistant-2017.4.3f1.exe");
				}

                // 一个总是有SynchronizationContext对象的是UI线程
                // 异步方法全部会回掉到主线程
                // Creates a new instance of the System.Threading.SynchronizationContext class
                SynchronizationContext.SetSynchronizationContext(OneThreadSynchronizationContext.Instance);

				DontDestroyOnLoad(gameObject);
				Game.EventSystem.Add(DLLType.Model, typeof(Init).Assembly);

				Game.Scene.AddComponent<GlobalConfigComponent>();
				Game.Scene.AddComponent<NetOuterComponent>();
				Game.Scene.AddComponent<ResourcesComponent>();
				Game.Scene.AddComponent<PlayerComponent>();
				Game.Scene.AddComponent<UnitComponent>();
				Game.Scene.AddComponent<ClientFrameComponent>();
				Game.Scene.AddComponent<UIComponent>();
                // 摄像机组件
                Game.Scene.AddComponent<CameraComponent>();
                // 场景切换(改为需要引用的时候在添加)
                //Game.Scene.AddComponent<SceneChangeComponent>();

                // 下载ab包
                await BundleHelper.DownloadBundle();

                // 加载热更新模块装配器, 并且绑定Hotfix的start为ETHotfix.Init的start
                Game.Hotfix.LoadHotfixAssembly();

				// 加载配置
				Game.Scene.GetComponent<ResourcesComponent>().LoadBundle("config.unity3d");
				Game.Scene.AddComponent<ConfigComponent>();
				Game.Scene.GetComponent<ResourcesComponent>().UnloadBundle("config.unity3d");
				Game.Scene.AddComponent<OpcodeTypeComponent>();
				Game.Scene.AddComponent<MessageDispatherComponent>();

                // 调用热更新模块的start方法
                Game.Hotfix.GotoHotfix();

                // TODO 这是测试事件，将来需要移除
                Game.EventSystem.Run(EventIdType.TestHotfixSubscribMonoEvent, "TestHotfixSubscribMonoEvent");
			}
			catch (Exception e)
			{
				Log.Error(e);
			}
		}

        // 所有的Update方法都是从这里开始执行的，下同理
        private void Update()
		{
			OneThreadSynchronizationContext.Instance.Update();
			Game.Hotfix.Update?.Invoke();
			Game.EventSystem.Update();
		}

		private void LateUpdate()
		{
			Game.Hotfix.LateUpdate?.Invoke();
			Game.EventSystem.LateUpdate();
		}

		private void OnApplicationQuit()
		{
			Game.Hotfix.OnApplicationQuit?.Invoke();
			Game.Close();
		}
	}
}