namespace Model
{
	public static class EventType
	{
		public const int BeforeAddBuff = 0;
		public const int AfterAddBuff = 1;
		public const int BeforeRemoveBuff = 2;
		public const int AfterRemoveBuff = 3;
		public const int BuffTimeout = 4;
		public const int LogicRecvMessage = 5;
		public const int GateRecvClientMessage = 6;
		public const int GateRecvServerMessage = 8;
	}
}