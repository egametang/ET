using System;
using System.Threading;

namespace ET.Client
{
	public static class Entry
	{
		public static void Start()
		{
			try
			{
				System.AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
				{
					Log.Error(e.ExceptionObject.ToString());
				};
				
				SynchronizationContext.SetSynchronizationContext(ThreadSynchronizationContext.Instance);
				
				CodeLoader.Instance.Update += Game.Update;
				CodeLoader.Instance.LateUpdate += Game.LateUpdate;
				CodeLoader.Instance.OnApplicationQuit += Game.Close;
				
				MongoHelper.Register(Game.EventSystem.GetTypes());
				
				Game.ILog = new UnityLogger();
				
				ETTask.ExceptionHandler += Log.Error;

				Options.Instance = new Options();

				Game.EventSystem.Publish(Game.Scene, new EventType.AppStart());
			}
			catch (Exception e)
			{
				Log.Error(e);
			}
		}
	}
}