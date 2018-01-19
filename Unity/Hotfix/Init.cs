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