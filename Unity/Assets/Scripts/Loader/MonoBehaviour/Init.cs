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
			
			// 添加线程同步处理功能
			Game.AddSingleton<MainThreadSynchronizationContext>();

			// 命令行参数
			string[] args = "".Split(" ");
			Parser.Default.ParseArguments<Options>(args)                                       // 将args数组中的元素解析为Options类型的对象
                .WithNotParsed(error => throw new Exception($"命令行格式错误! {error}"))       // 如果解析失败，调用WithNotParsed方法，抛出一个异常，异常信息包含错误原因
                .WithParsed(Game.AddSingleton);                                                // 如果解析成功，调用WithParsed方法，将解析得到的Options对象作为参数传递给Game.AddSingleton方法

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

		/// <summary>
		/// 每帧调用一次
		/// </summary>
		private void Update()
		{
			// 更新游戏中的单例对象
			Game.Update();
		}

		/// <summary>
		/// 每帧结束时调用一次
		/// </summary>
		private void LateUpdate()
		{
			// 更新游戏中的单例对象
			Game.LateUpdate();
			Game.FrameFinishUpdate();
		}

		/// <summary>
		/// 应用程序退出
		/// </summary>
		private void OnApplicationQuit()
		{
			// 销毁单例对象
			Game.Close();
		}
	}
}