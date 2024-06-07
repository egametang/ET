namespace ET
{
	public static partial class SceneType
	{
		public const int Location = PackageType.StateSync * 1000 + 1;
		public const int Realm = PackageType.StateSync * 1000 + 2;
		public const int Gate = PackageType.StateSync * 1000 + 3;
		public const int Http = PackageType.StateSync * 1000 + 4;
		public const int Map = PackageType.StateSync * 1000 + 5;
		public const int Router = PackageType.StateSync * 1000 + 6;
		public const int RouterManager = PackageType.StateSync * 1000 + 7;
		public const int Robot = PackageType.StateSync * 1000 + 8;
		public const int Watcher = PackageType.StateSync * 1000 + 9;
		public const int GameTool = PackageType.StateSync * 1000 + 10;
		public const int Server = PackageType.StateSync * 1000 + 11;
		

		// 客户端
		public const int StateSync = PackageType.StateSync * 1000 + 20;
		public const int Current = PackageType.StateSync * 1000 + 21;
		public const int StateSyncView = PackageType.StateSync * 1000 + 24;
	}
}