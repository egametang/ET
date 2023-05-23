using System;
using System.Collections;
using CommandLine;
using UnityEngine;
using YooAsset;

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
			
			GlobalConfig globalConfig = Resources.Load<GlobalConfig>("GlobalConfig");
			GlobalConfig.Instance = globalConfig;
			ETTask.ExceptionHandler += Log.Error;
			YooAssets.Initialize();
			StartCoroutine(InitPackage(StartProgram));
		}

		void StartProgram()
		{
			Game.AddSingleton<CodeLoader>().Start();
		}
		
		
		private IEnumerator InitPackage(Action actSuc)
		{
			//yield return new WaitForSeconds(1f);

			// 创建默认的资源包
			string packageName = "DefaultPackage";
			var package = YooAssets.GetPackage(packageName);
			if (package == null)
			{
				package = YooAssets.CreatePackage(packageName);
				YooAssets.SetDefaultPackage(package);
			}

			var playMode = GlobalConfig.Instance.PlayMode;
			// 编辑器下的模拟模式
			InitializationOperation initializationOperation = null;
			if (playMode == EPlayMode.EditorSimulateMode)
			{
				var createParameters = new EditorSimulateModeParameters();
				createParameters.LocationToLower = true;
				createParameters.SimulateManifestFilePath = EditorSimulateModeHelper.SimulateBuild(packageName);
				initializationOperation = package.InitializeAsync(createParameters);
			}

			// 单机运行模式
			if (playMode == EPlayMode.OfflinePlayMode)
			{
				var createParameters = new OfflinePlayModeParameters();
				createParameters.LocationToLower = true;
				initializationOperation = package.InitializeAsync(createParameters);
			}

			// 联机运行模式
			if (playMode == EPlayMode.HostPlayMode)
			{
				var createParameters = new HostPlayModeParameters();
				createParameters.LocationToLower = true;
				//createParameters.QueryServices = new GameQueryServices();
				//createParameters.DefaultHostServer = "http://127.0.0.1/Default/StandaloneWindows64/v1.0"; ;
				//createParameters.FallbackHostServer = GetHostServerURL();
				initializationOperation = package.InitializeAsync(createParameters);
			}

			yield return initializationOperation;
			if (package.InitializeStatus == EOperationStatus.Succeed)
			{
				actSuc?.Invoke();
			}
			else
			{
				Debug.LogWarning($"{initializationOperation.Error}");
			}
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