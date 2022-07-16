using System.Threading;
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
			DontDestroyOnLoad(gameObject);

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