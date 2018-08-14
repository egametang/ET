namespace ETModel
{
#if UNITY_EDITOR
    public enum PlatformType
    {
        None,
        Android,
        IOS,
        PC,
        MacOS,
    }
#endif

    public static class Define
	{
#if UNITY_EDITOR && !ASYNC
		public static bool IsAsync = false;
#else
        public static bool IsAsync = true;
#endif

#if UNITY_EDITOR
		public static bool IsEditorMode = true;
#else
		public static bool IsEditorMode = false;
#endif

#if DEVELOPMENT_BUILD
		public static bool IsDevelopmentBuild = true;
#else
		public static bool IsDevelopmentBuild = false;
#endif

#if ILRuntime
		public static bool IsILRuntime = true;
#else
		public static bool IsILRuntime = false;
#endif

#if UNITY_EDITOR
        public static string AssetBundleBuildFolder = "../Release/{0}/StreamingAssets/";

        public static PlatformType GetPlatformType()
        {

    #if UNITY_ANDROID
                return PlatformType.Android;

    #elif UNITY_IOS
                return PlatformType.IOS;

    #elif UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
                return PlatformType.PC;

    #elif UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
                return PlatformType.MacOS;

    #else
                return PlatformType.None;
    #endif
        }
#endif
    }
}