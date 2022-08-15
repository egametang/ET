using System;
using System.Threading;

namespace ET.Client
{
	public static class Entry
	{
		public static void Start()
		{
			StartAsync().Coroutine();
		}
		
		private static async ETTask StartAsync()
		{
			try
			{
				AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
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

				await Game.EventSystem.Callback<InitCallback, ETTask>(new InitCallback() {Id = InitCallbackId.InitShare});
				
				switch (CodeLoader.Instance.GlobalConfig.CodeMode)
				{
					case CodeMode.Client:
						await Game.EventSystem.Callback<InitCallback, ETTask>(new InitCallback() {Id = InitCallbackId.InitClient});
						break;
					case CodeMode.Server:
						await Game.EventSystem.Callback<InitCallback, ETTask>(new InitCallback() {Id = InitCallbackId.InitServer});
						break;
					case CodeMode.ClientServer:
						await Game.EventSystem.Callback<InitCallback, ETTask>(new InitCallback() {Id = InitCallbackId.InitServer});
						await Game.EventSystem.Callback<InitCallback, ETTask>(new InitCallback() {Id = InitCallbackId.InitClient});
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
				Log.Info("Init Finish!");
			}
			catch (Exception e)
			{
				Log.Error(e);
			}
		}
	}
}