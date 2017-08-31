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
				Hotfix.Scene.ModelScene.AddComponent<OpcodeTypeComponent>();
				Hotfix.Scene.ModelScene.AddComponent<Model.MessageDispatherComponent>();
				Hotfix.Scene.ModelScene.AddComponent<NetOuterComponent>();
				Hotfix.Scene.ModelScene.AddComponent<ResourcesComponent>();
				Hotfix.Scene.ModelScene.AddComponent<BehaviorTreeComponent>();  
				Hotfix.Scene.AddComponent<UIComponent>();
				Hotfix.Scene.GetComponent<EventComponent>().Run(EventIdType.InitSceneStart);
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
				ObjectEvents.Instance.Update();
			}
			catch (Exception e)
			{
				Log.Error(e.ToString());
			}
		}

		public static void OnApplicationQuit()
		{
			Hotfix.Close();
		}
	}
}