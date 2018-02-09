namespace Model
{
	public static class EventIdType
	{
		public const int RecvHotfixMessage = 1;

		public const int BehaviorTreeRunTreeEvent = 2;
		public const int BehaviorTreeOpenEditor = 3;
		public const int BehaviorTreeClickNode = 4;
		public const int BehaviorTreeAfterChangeNodeType = 5;
		public const int BehaviorTreeCreateNode = 6;
		public const int BehaviorTreePropertyDesignerNewCreateClick = 7;
		public const int BehaviorTreeMouseInNode = 8;
		public const int BehaviorTreeConnectState = 9;
		public const int BehaviorTreeReplaceClick = 10;
		public const int BehaviorTreeRightDesignerDrag = 11;

		public const int SessionRecvMessage = 12;
		public const int NumbericChange = 13;

		public const int MessageDeserializeFinish = 14;
		public const int SceneChange = 15;
		public const int FrameUpdate = 16;


		public const int LoadingBegin = 17;
		public const int LoadingFinish = 18;

		public const int TestHotfixSubscribMonoEvent = 19;

		public const int MaxModelEvent = 10000;
	}
}