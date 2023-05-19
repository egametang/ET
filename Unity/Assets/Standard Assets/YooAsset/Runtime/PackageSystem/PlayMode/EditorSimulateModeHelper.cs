#if UNITY_EDITOR
using System.Reflection;

namespace YooAsset
{
	public static class EditorSimulateModeHelper
	{
		private static System.Type _classType;

		/// <summary>
		/// 编辑器下模拟构建清单
		/// </summary>
		public static string SimulateBuild(string packageName)
		{
			if (_classType == null)
				_classType = Assembly.Load("YooAsset.Editor").GetType("YooAsset.Editor.AssetBundleSimulateBuilder");

			string manifestFilePath = (string)InvokePublicStaticMethod(_classType, "SimulateBuild", packageName);
			return manifestFilePath;
		}

		private static object InvokePublicStaticMethod(System.Type type, string method, params object[] parameters)
		{
			var methodInfo = type.GetMethod(method, BindingFlags.Public | BindingFlags.Static);
			if (methodInfo == null)
			{
				UnityEngine.Debug.LogError($"{type.FullName} not found method : {method}");
				return null;
			}
			return methodInfo.Invoke(null, parameters);
		}
	}
}
#else
namespace YooAsset
{ 
	public static class EditorSimulateModeHelper
	{
		/// <summary>
		/// 编辑器下模拟构建清单
		/// </summary>
		public static string SimulateBuild(string packageName) { throw new System.Exception("Only support in unity editor !"); }
	}
}
#endif