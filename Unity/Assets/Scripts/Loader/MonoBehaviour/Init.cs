using System;
using CommandLine;
using UnityEngine;

namespace ET
{
	/// <summary>
	/// 整个框架入口 继承MonoBehaviour
	/// </summary>
	public class Init: MonoBehaviour
	{
		private void Start()
		{
			this.StartAsync().Coroutine();
		}
		
		private async ETTask StartAsync()
		{
			//防止切换场景后销毁
			DontDestroyOnLoad(gameObject);
			
			//监听未处理的异常，然后打印
			AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
			{
				Log.Error(e.ExceptionObject.ToString());
			};

			// 服务器启动服务的命令行参数
			string[] args = "".Split(" ");
			//解析配置
			Parser.Default.ParseArguments<Options>(args)
				.WithNotParsed(error => throw new Exception($"命令行格式错误! {error}"))
				.WithParsed((o)=>World.Instance.AddSingleton(o));
			//开始游戏配置的文件夹
			Options.Instance.StartConfig = $"StartConfig/Localhost";
			
			//添加Logger单例
			World.Instance.AddSingleton<Logger>().Log = new UnityLogger();
			ETTask.ExceptionHandler += Log.Error;
			
			//添加时间信息类单例
			World.Instance.AddSingleton<TimeInfo>();
			//添加纤程管理类单例
			World.Instance.AddSingleton<FiberManager>();

			//异步等待添加资源加载组件单例，并且执行YooAsset资源包初始化
			await World.Instance.AddSingleton<ResourcesComponent>().CreatePackageAsync("DefaultPackage", true);
			
			//添加代码加载器单例
			CodeLoader codeLoader = World.Instance.AddSingleton<CodeLoader>();
			//执行代码热更文件的加载
			await codeLoader.DownloadAsync();
			//执行代码热更
			codeLoader.Start();
		}

		private void Update()
		{
			//更新时间信息
			TimeInfo.Instance.Update();
			//更新纤程管理器
			FiberManager.Instance.Update();
		}

		private void LateUpdate()
		{
			//更新纤程管理器
			FiberManager.Instance.LateUpdate();
		}

		private void OnApplicationQuit()
		{
			//关闭，移除全部单例类
			World.Instance.Dispose();
		}
	}
}