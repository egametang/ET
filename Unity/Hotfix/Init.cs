using System;
using ETModel;

namespace ETHotfix
{
	public static class Init
	{
		public static void Start()
		{
			try
			{
				Game.Scene.ModelScene = ETModel.Game.Scene;

				// 注册热更层回调
				ETModel.Game.Hotfix.Update = () => { Update(); };
				ETModel.Game.Hotfix.LateUpdate = () => { LateUpdate(); };
				ETModel.Game.Hotfix.OnApplicationQuit = () => { OnApplicationQuit(); };

				// 注册热更层消息回调
				ClientDispatcher clientDispatcher = new ClientDispatcher
				{
					HotfixCallback = (s, p) => { HotfixMessageDispatcher.Run(s, p); }
				};
				ETModel.Game.Scene.GetComponent<NetOuterComponent>().MessageDispatcher = clientDispatcher;

				Game.Scene.AddComponent<UIComponent>();
				Game.Scene.AddComponent<OpcodeTypeComponent>();
				Game.Scene.AddComponent<MessageDispatherComponent>();

				// 加载热更配置
				ETModel.Game.Scene.GetComponent<ResourcesComponent>().LoadBundle("config.unity3d");
				Game.Scene.AddComponent<ConfigComponent>();
				ETModel.Game.Scene.GetComponent<ResourcesComponent>().UnloadBundle("config.unity3d");

				Game.EventSystem.Run(EventIdType.InitSceneStart);
			}
			catch (Exception e)
			{
				Log.Error(e.ToStr());
			}
		}

		public static void Update()
		{
			try
			{
				Game.EventSystem.Update();
			}
			catch (Exception e)
			{
				Log.Error(e.ToStr());
			}
		}

		public static void LateUpdate()
		{
			try
			{
				Game.EventSystem.LateUpdate();
			}
			catch (Exception e)
			{
				Log.Error(e.ToStr());
			}
		}

		public static void OnApplicationQuit()
		{
			Game.Close();
		}
	}
}