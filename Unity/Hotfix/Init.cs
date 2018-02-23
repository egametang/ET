using System;
using Model;

namespace Hotfix
{
	public static class Init
	{
		public static void Start()
		{
			try
			{
				Hotfix.Scene.ModelScene = Game.Scene;

				// 注册热更层回调
				Game.Hotfix.Update = () => { Update(); };
				Game.Hotfix.LateUpdate = () => { LateUpdate(); };
				Game.Hotfix.OnApplicationQuit = () => { OnApplicationQuit(); };

				// 注册热更层消息回调
				ClientDispatcher clientDispatcher = new ClientDispatcher
				{
					HotfixCallback = (s, p) => { HotfixMessageDispatcher.Run(s, p); }
				};
				Game.Scene.GetComponent<NetOuterComponent>().MessageDispatcher = clientDispatcher;

				Hotfix.Scene.AddComponent<UIComponent>();
				Hotfix.Scene.AddComponent<OpcodeTypeComponent>();
				Hotfix.Scene.AddComponent<MessageDispatherComponent>();

				Hotfix.EventSystem.Run(EventIdType.InitSceneStart);
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
				Hotfix.EventSystem.Update();
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
				Hotfix.EventSystem.LateUpdate();
			}
			catch (Exception e)
			{
				Log.Error(e.ToStr());
			}
		}

		public static void OnApplicationQuit()
		{
			Hotfix.Close();
		}
	}
}