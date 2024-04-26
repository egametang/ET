namespace ET
{
	[UniqueId]
	public static partial class SceneType
	{
		public const int Location = 3;
		public const int Realm = 4;
		public const int Gate = 5;
		public const int Http = 6;
		public const int Map = 8;
		public const int Router = 9;
		public const int RouterManager = 10;
		public const int Robot = 11;
		public const int Watcher = 19;
		public const int GameTool = 20;
		public const int Server = 21;
		

		// 客户端
		public const int StateSync = 50;
		public const int Current = 51;
		public const int StateSyncView = 54;
	}
}