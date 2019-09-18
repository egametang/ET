namespace ETModel
{
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
	}
}