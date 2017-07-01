using System;
using Model;

namespace Hotfix
{
	public static class Init
	{
		private static void Start()
		{
			try
			{
				Hotfix.Scene.ModelScene.AddComponent<OpcodeTypeComponent>();
				Hotfix.Scene.ModelScene.AddComponent<NetOuterComponent>();
				Hotfix.Scene.ModelScene.AddComponent<ResourcesComponent>();
				Hotfix.Scene.ModelScene.AddComponent<BehaviorTreeComponent>();  
				Hotfix.Scene.AddComponent<UIComponent>();
				Hotfix.Scene.AddComponent<UnitComponent>();
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