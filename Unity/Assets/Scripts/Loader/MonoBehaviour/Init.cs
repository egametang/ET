using System;
using System.Threading;
using CommandLine;
using UnityEngine;

namespace ET
{
	// 客户端初始化脚本
	public class Init: MonoBehaviour
	{
		private void Start()
		{
            // 使当前游戏对象在加载新场景时不被销毁
            DontDestroyOnLoad(gameObject);

            // 为当前应用程序域添加未处理异常的事件处理器
            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
			{
                // 记录异常信息到日志中
                Log.Error(e.ExceptionObject.ToString());
			};
				
			Game.AddSingleton<MainThreadSynchronizationContext>();

			// 命令行参数
			string[] args = "".Split(" ");
			Parser.Default.ParseArguments<Options>(args)
				.WithNotParsed(error => throw new Exception($"命令行格式错误! {error}"))
				.WithParsed(Game.AddSingleton);
			
			Game.AddSingleton<TimeInfo>();
			Game.AddSingleton<Logger>().ILog = new UnityLogger();
			Game.AddSingleton<ObjectPool>();
			Game.AddSingleton<IdGenerater>();
			Game.AddSingleton<EventSystem>();
			Game.AddSingleton<TimerComponent>();
			Game.AddSingleton<CoroutineLockComponent>();
			
			ETTask.ExceptionHandler += Log.Error;

			Game.AddSingleton<CodeLoader>().Start();
		}

		private void Update()
		{
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