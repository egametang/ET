﻿using System;
using System.Threading;
using CommandLine;
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

			// 命令行参数
			string[] args = "".Split(" ");
			Parser.Default.ParseArguments<Options>(args)
				.WithNotParsed(error => throw new Exception($"命令行格式错误! {error}"))
				.WithParsed(Game.AddSingleton);
			
			Game.AddSingleton<RandomGenerator>();
			Game.AddSingleton<TimeInfo>();
			Game.AddSingleton<Logger>().ILog = new UnityLogger();
			Game.AddSingleton<ObjectPool>();
			Game.AddSingleton<IdGenerater>();
			Game.AddSingleton<EventSystem>();
			Game.AddSingleton<TimerComponent>();
			Game.AddSingleton<CoroutineLockComponent>();
			Game.AddSingleton<NetServices>();
			Game.AddSingleton<Root>();
			
			ETTask.ExceptionHandler += Log.Error;

			Game.AddSingleton<CodeLoader>().Start();
		}

		private void Update()
		{
			ThreadSynchronizationContext.Instance.Update();
			Game.Update();
		}

		private void LateUpdate()
		{
			Game.LateUpdate();
			Game.FrameFinishUpdate();
		}

		private void OnApplicationQuit()
		{
			Game.Close();
		}
	}
}