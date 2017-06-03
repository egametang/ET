using System;
using Model;

namespace Hotfix
{
	public static class HotfixInit
	{
		private static void Start()
		{
			try
			{
				Game.Scene.AddComponent<ResourcesComponent>();
				Game.Scene.AddComponent<UIComponent>();
				Game.Scene.AddComponent<UnitComponent>();
				Game.Scene.AddComponent<MessageDispatherComponent, AppType>(AppType.Client);
				Game.Scene.AddComponent<NetOuterComponent>();
				Game.Scene.GetComponent<EventComponent>().Run(EventIdType.InitSceneStart);
			}
			catch (Exception e)
			{
				Log.Error(e.ToString());
			}
		}

		private static void Update()
		{
			try
			{
				Game.EntityEventManager.Update();
			}
			catch (Exception e)
			{
				Log.Error(e.ToString());
			}
		}

		private static void OnApplicationQuit()
		{
			Game.CloseScene();
		}
	}
}