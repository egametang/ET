using System.Threading;
using UnityEngine;

namespace ET
{
	public class Init: MonoBehaviour
	{
		public GlobalConfig GlobalConfig;
		
		private void Awake()
		{
			DontDestroyOnLoad(gameObject);

			CodeLoader.Instance.GlobalConfig = this.GlobalConfig;
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