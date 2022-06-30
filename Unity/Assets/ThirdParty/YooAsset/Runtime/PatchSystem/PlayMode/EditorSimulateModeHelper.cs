#if UNITY_EDITOR
using System.Reflection;

namespace YooAsset
{
	internal static class EditorSimulateModeHelper
	{
		private static System.Type _classType;

		public static string SimulateBuild()
		{
			_classType = Assembly.Load("YooAsset.Editor").GetType("YooAsset.Editor.AssetBundleSimulateBuilder");
			InvokePublicStaticMethod(_classType, "SimulateBuild");
			return GetPatchManifestFilePath();
		}
		private static string GetPatchManifestFilePath()
		{
			return (string)InvokePublicStaticMethod(_classType, "GetPatchManifestPath");
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
	internal static class EditorSimulateModeHelper
	{
		public static string SimulateBuild() { throw new System.Exception("Only support in unity editor !"); }
	}
#endif