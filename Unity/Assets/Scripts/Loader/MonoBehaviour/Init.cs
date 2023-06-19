using System;
using CommandLine;
using UnityEngine;

namespace ET
{
	public class Init: MonoBehaviour
	{
		private void Start()
		{
			DontDestroyOnLoad(gameObject);
			
			AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
			{
				Log.Error(e.ExceptionObject.ToString());
			};

			// 命令行参数
			string[] args = "".Split(" ");
			Parser.Default.ParseArguments<Options>(args)
				.WithNotParsed(error => throw new Exception($"命令行格式错误! {error}"))
				.WithParsed(World.Instance.AddSingleton);
			Options.Instance.StartConfig = $"StartConfig/Localhost";
			
			World.Instance.AddSingleton<Logger>().ILog = new UnityLogger();
			ETTask.ExceptionHandler += Log.Error;
			World.Instance.AddSingleton<OpcodeType>();
			World.Instance.AddSingleton<IdValueGenerater>();
			World.Instance.AddSingleton<ObjectPool>();
			World.Instance.AddSingleton<WorldActor>();
			World.Instance.AddSingleton<CodeLoader>();
			World.Instance.AddSingleton<VProcessManager>();
			VProcessManager.MainThreadScheduler mainThreadScheduler = World.Instance.AddSingleton<VProcessManager.MainThreadScheduler>();

			int vProcessId = VProcessManager.Instance.Create();
			mainThreadScheduler.Add(vProcessId);
			
			// 发送消息
			WorldActor.Instance.Send(new ActorId(Options.Instance.Process, vProcessId, 0), null);
		}

		private void Update()
		{
			VProcessManager.MainThreadScheduler.Instance.Update();
		}

		private void LateUpdate()
		{
			VProcessManager.MainThreadScheduler.Instance.LateUpdate();
		}

		private void OnApplicationQuit()
		{
			World.Instance.Dispose();
		}
	}
	
	
}