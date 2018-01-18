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
				ObjectSystem objectSystem = ObjectSystem.Instance;
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
				ObjectSystem.Instance.Update();
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
				ObjectSystem.Instance.LateUpdate();
			}
			catch (Exception e)
			{
				Log.Error(e.ToStr());
			}
		}

		public static void OnApplicationQuit()
		{
			ObjectSystem.Close();
			Hotfix.Close();
		}
	}
}