namespace ET
{
	[UniqueId]
	public static partial class SceneType
	{
		public const int All = 0;
		public const int Server = 1;
		public const int Main = 2; // 主纤程,一个进程一个, 初始化从这里开始
		public const int NetInner = 3; // 负责进程间消息通信
		public const int Realm = 4;
		public const int Gate = 5;
		public const int Http = 6;
		public const int Location = 7;
		public const int Map = 8;
		public const int Router = 9;
		public const int RouterManager = 10;
		public const int Robot = 11;
		public const int BenchmarkClient = 12;
		public const int BenchmarkServer = 13;
		public const int Match = 14;
		public const int Room = 15;
		public const int LockStepClient = 16;
		public const int LockStepServer = 17;
		public const int RoomRoot = 18;
		public const int Watcher = 19;
		public const int GameTool = 20;
		

		// 客户端
		public const int StateSync = 50;
		public const int Current = 51;
		public const int LockStep = 52;
		public const int LockStepView = 53;
		public const int StateSyncView = 54;
		public const int NetClient = 55;
	}
}