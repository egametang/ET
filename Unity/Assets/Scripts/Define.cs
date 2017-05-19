namespace Model
{
	public enum LoadResourceType
	{
		Async,
		Sync
	}

	public static class Define
	{
		public const int FlyStartV = 7;
		public const int GravityAcceleration = 36;
#if UNITY_EDITOR && !ASYNC
		public static LoadResourceType LoadResourceType = LoadResourceType.Sync;
#else
        public static LoadResourceType LoadResourceType = LoadResourceType.Async;
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
	}
}