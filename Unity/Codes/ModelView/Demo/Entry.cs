using System;

namespace ET
{
	public static class Entry
	{
		public static void Start()
		{
			try
			{
				CodeLoader.Instance.Update += Game.Update;
				CodeLoader.Instance.LateUpdate += Game.LateUpdate;
				CodeLoader.Instance.OnApplicationQuit += Game.Close;
				
				Log.Info($"11111111111111111111111111111111111111111111111");
				
				Game.EventSystem.Add(CodeLoader.Instance.GetTypes());

				Log.Info($"11111111111111111111111111111111111111111111112");
				
				Game.EventSystem.Publish(new EventType.AppStart()).Coroutine();
			}
			catch (Exception e)
			{
				Log.Error(e);
			}
		}
	}
}