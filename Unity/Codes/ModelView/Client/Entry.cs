using System;

namespace ET.Client
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
				
				
				Game.EventSystem.Add(CodeLoader.Instance.GetTypes());

				
				Game.EventSystem.Publish(Game.Scene, new EventType.AppStart());
			}
			catch (Exception e)
			{
				Log.Error(e);
			}
		}
	}
}