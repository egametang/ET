using System.Threading;
using UnityEngine;

namespace ET
{
	// 1 mono模式 2 ILRuntime模式 3 mono热重载模式
	public enum CodeMode
	{
		Mono = 1,
		ILRuntime = 2,
		Reload = 3,
	}
	
	public class Init: MonoBehaviour
	{
		public static Init Instance;
		
		private CodeLoader codeLoader;

		public CodeMode CodeMode = CodeMode.Mono;
		
		private void Awake()
		{
			Instance = this;
			
#if ENABLE_IL2CPP
			this.CodeMode = CodeMode.ILRuntime;
#endif
			
			System.AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
			{
				Log.Error(e.ExceptionObject.ToString());
			};
			
			SynchronizationContext.SetSynchronizationContext(ThreadSynchronizationContext.Instance);
			
			DontDestroyOnLoad(gameObject);

			Log.ILog = new UnityLogger();

			Options.Instance = new Options();

			this.codeLoader = CodeLoader.Instance;
		}

		private void Start()
		{
			this.codeLoader.Start();
		}

		private void Update()
		{
			this.codeLoader.Update();
		}

		private void LateUpdate()
		{
			this.codeLoader.LateUpdate();
		}

		private void OnApplicationQuit()
		{
			this.codeLoader.OnApplicationQuit();
		}
	}
}