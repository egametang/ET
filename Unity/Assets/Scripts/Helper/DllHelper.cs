using System.Reflection;
using UnityEngine;

namespace Model
{
	public static class DllHelper
	{
		public static Assembly GetController()
		{
			GameObject code = (GameObject)Resources.Load("Code");
			byte[] assBytes = code.Get<TextAsset>("Controller.dll").bytes;
			byte[] mdbBytes = code.Get<TextAsset>("Controller.dll.mdb").bytes;
			Assembly assembly = Assembly.Load(assBytes, mdbBytes);
			return assembly;
		}
	}
}
