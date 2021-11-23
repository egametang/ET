using System.Threading;
using UnityEngine;

namespace ET
{
	public class Init: MonoBehaviour
	{
		private CodeLoader codeLoader;
		
		private void Awake()
		{
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
			this.codeLoader.Update.Invoke();
		}

		private void LateUpdate()
		{
			this.codeLoader.LateUpdate.Invoke();
		}

		private void OnApplicationQuit()
		{
			this.codeLoader.OnApplicationQuit.Invoke();
		}
	}
}