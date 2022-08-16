using System;
using System.Threading;
using UnityEngine;

namespace ET
{
	public class Init: MonoBehaviour
	{
		public static Init Instance;
		
		public GlobalConfig GlobalConfig;
		
		private void Awake()
		{
			Instance = this;
			
			DontDestroyOnLoad(gameObject);
			
			AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
			{
				Log.Error(e.ExceptionObject.ToString());
			};
				
			SynchronizationContext.SetSynchronizationContext(ThreadSynchronizationContext.Instance);
			
			Game.ILog = new UnityLogger();
				
			ETTask.ExceptionHandler += Log.Error;

			Options.Instance = new Options();
			
			CodeLoader.Instance.Start();
		}

		private void Update()
		{
			Game.Update();
		}

		private void LateUpdate()
		{
			Game.LateUpdate();
		}

		private void OnApplicationQuit()
		{
			Game.Close();
			CodeLoader.Instance.Dispose();
		}
	}
}