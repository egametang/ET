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
			ObjectEvents.Instance.RegisterILRuntime();
			ObjectEvents.Instance.RegisterILAdapter();
#else
			ObjectEvents.Instance.Register("Hotfix", DllHelper.LoadHotfixAssembly());
#endif
			ObjectEvents.Instance.Register("Model", typeof (Game).Assembly);
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
				ObjectEvents.Instance.Update();
			}
			catch (Exception e)
			{
				Log.Error(e.ToString());
			}
		}

		private void OnApplicationQuit()
		{
			Game.Close();
		}
	}
}