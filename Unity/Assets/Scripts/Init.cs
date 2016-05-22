using System;
using System.Reflection;
using UnityEngine;

namespace Base
{
	public class Init : MonoBehaviour
	{
		public Action UpdateAction;

		private void Start()
		{
			GameObject code = (GameObject)Resources.Load("Code/Code");
			byte[] assBytes = code.Get<TextAsset>("Controller.dll").bytes;
			byte[] mdbBytes = code.Get<TextAsset>("Controller.dll.mdb").bytes;
			Assembly assembly = Assembly.Load(assBytes, mdbBytes);

			MethodInfo methodInfo = assembly.GetType("Controller.Init").GetMethod("Start");
			methodInfo.Invoke(null, null);
		}

		private void Update()
		{
			this.UpdateAction?.Invoke();
		}
	}
}

