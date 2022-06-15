﻿using System.Threading;
using UnityEngine;

namespace ET
{
	// 1 mono模式 2 mono热重载模式
	public enum CodeMode
	{
		Mono = 1,
		Reload = 2,
	}
	
	public class Init: MonoBehaviour
	{
		public CodeMode CodeMode = CodeMode.Mono;
		
		private void Awake()
		{
#if ENABLE_IL2CPP
			this.CodeMode = CodeMode.ILRuntime;
#endif
			
			System.AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
			{
				Log.Error(e.ExceptionObject.ToString());
			};
			
			SynchronizationContext.SetSynchronizationContext(ThreadSynchronizationContext.Instance);
			
			DontDestroyOnLoad(gameObject);

			ETTask.ExceptionHandler += Log.Error;

			Game.ILog = new UnityLogger();

			Options.Instance = new Options();

			CodeLoader.Instance.CodeMode = this.CodeMode;
		}

		private void Start()
		{
			CodeLoader.Instance.Start();
		}

		private void Update()
		{
			CodeLoader.Instance.Update();
		}

		private void LateUpdate()
		{
			CodeLoader.Instance.LateUpdate();
		}

		private void OnApplicationQuit()
		{
			CodeLoader.Instance.OnApplicationQuit();
			CodeLoader.Instance.Dispose();
		}
	}
}