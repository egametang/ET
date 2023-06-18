namespace ET
{
	public static class Define
	{
		public const string CodeDir = "Assets/Bundles/Code/";
		public const string BuildOutputDir = "Temp/Bin/Debug";
#if DEBUG
		public static bool IsDebug = true;
#else
		public static bool IsDebug = false;
#endif
		
#if UNITY_EDITOR && !ASYNC
		public static bool IsAsync = false;
#else
        public static bool IsAsync = true;
#endif
		
#if UNITY_EDITOR
		public static bool IsEditor = true;
#else
        public static bool IsEditor = false;
#endif
		
#if ENABLE_DLL
		public static bool EnableDll = true;
#else
        public static bool EnableDll = false;
#endif
		
#if ENABLE_VIEW
		public static bool EnableView = true;
#else
		public static bool EnableView = false;
#endif
		
#if ENABLE_IL2CPP
		public static bool EnableIL2CPP = true;
#else
		public static bool EnableIL2CPP = false;
#endif
		
		public static UnityEngine.Object LoadAssetAtPath(string s)
		{
#if UNITY_EDITOR	
			return UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(s);
#else
			return null;
#endif
		}
		
		public static string[] GetAssetPathsFromAssetBundle(string assetBundleName)
		{
#if UNITY_EDITOR	
			return UnityEditor.AssetDatabase.GetAssetPathsFromAssetBundle(assetBundleName);
#else
			return new string[0];
#endif
		}
		
		public static string[] GetAssetBundleDependencies(string assetBundleName, bool v)
		{
#if UNITY_EDITOR	
			return UnityEditor.AssetDatabase.GetAssetBundleDependencies(assetBundleName, v);
#else
			return new string[0];
#endif
		}
	}
}