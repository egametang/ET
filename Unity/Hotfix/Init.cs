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
				Log.Error(e.ToStr());
			}
		}

		public static void LateUpdate()
		{
			try
			{
				ObjectEvents.Instance.LateUpdate();
			}
			catch (Exception e)
			{
				Log.Error(e.ToStr());
			}
		}

		public static void OnApplicationQuit()
		{
			ObjectEvents.Close();
			Hotfix.Close();
		}
	}
}