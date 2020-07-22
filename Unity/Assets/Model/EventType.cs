namespace ET
{
	namespace EventType
	{
		public struct LoginFinish
		{}

		public struct LoadingBegin
		{
			public Scene Scene;
		}

		public struct LoadingFinish
		{
		}
		
		public struct EnterMapFinish
		{}

		public struct AfterUnitCreate
		{
			public Unit Unit;
		}
	}
}