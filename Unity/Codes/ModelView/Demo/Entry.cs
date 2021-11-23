using System;

namespace ET
{
	public static class Entry
	{
		public static void Start()
		{
			try
			{
				Game.EventSystem.Add(typeof(Entry).Assembly);

				CodeLoader.Instance.Update = Game.Update;
				CodeLoader.Instance.LateUpdate = Game.LateUpdate;
				CodeLoader.Instance.OnApplicationQuit = Game.Close;
				
				ProtobufHelper.Init();
				
				Game.EventSystem.Publish(new EventType.AppStart()).Coroutine();
			}
			catch (Exception e)
			{
				Log.Error(e);
			}
		}
	}
}