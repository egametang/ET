namespace ET
{
	[UniqueId]
	public static partial class SceneType
	{
		public const int All = 0;
		public const int Main = 1; // 主纤程,一个进程一个, 初始化从这里开始
		public const int NetInner = 2; // 负责进程间消息通信
		public const int Realm = 3;
		public const int Gate = 4;
		public const int Http = 5;
		public const int Location = 6;
		public const int Map = 7;
		public const int Router = 8;
		public const int RouterManager = 9;
		public const int Robot = 10;
		public const int BenchmarkClient = 11;
		public const int BenchmarkServer = 12;
		public const int Match = 13;
		public const int Room = 14;
		public const int LockStepClient = 15;
		public const int LockStepServer = 16;
		public const int RoomRoot = 17;
		public const int Watcher = 18;

		// 客户端
		public const int StateSync = 50;
		public const int Current = 51;
		public const int LockStep = 52;
		public const int LockStepView = 53;
		public const int StateSyncView = 54;
		public const int NetClient = 55;
	}
}