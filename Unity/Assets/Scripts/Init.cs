using System;
using Base;
using UnityEngine;

namespace Model
{
	public class Init: MonoBehaviour
	{
		private void Start()
		{
#if ILRuntime
			Game.EntityEventManager.RegisterILRuntime();
			Game.EntityEventManager.RegisterILAdapter();
#else
			Game.EntityEventManager.Register("Hotfix", DllHelper.LoadHotfixAssembly());
#endif
			Game.EntityEventManager.Register("Model", typeof (Game).Assembly);
			Game.Scene.AddComponent<ResourcesComponent>();
			Game.Scene.AddComponent<UIComponent>();
			Game.Scene.AddComponent<UnitComponent>();
			Game.Scene.AddComponent<BehaviorTreeComponent>();
			Game.Scene.AddComponent<MessageDispatherComponent, AppType>(AppType.Client);
			Game.Scene.AddComponent<NetOuterComponent>();

			EventHelper.Run(EventIdType.InitSceneStart);
		}

		private void Update()
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

		private void OnApplicationQuit()
		{
			Game.CloseScene();
		}
	}
}