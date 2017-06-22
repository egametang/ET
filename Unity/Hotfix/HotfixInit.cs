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
				Hotfix.Scene.AddComponent<ResourcesComponent>();
				Hotfix.Scene.AddComponent<UIComponent>();
				Hotfix.Scene.AddComponent<UnitComponent>();
				Hotfix.Scene.AddComponent<MessageDispatherComponent, AppType>(AppType.Client);
				Hotfix.Scene.AddComponent<NetOuterComponent>();
				Hotfix.Scene.GetComponent<EventComponent>().Run(EventIdType.InitSceneStart);
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
				ObjectEvents.Instance.Update();
			}
			catch (Exception e)
			{
				Log.Error(e.ToString());
			}
		}

		private static void OnApplicationQuit()
		{
			Hotfix.Close();
		}
	}
}