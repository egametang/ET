namespace Model
{
	public enum EventType
	{
		BeforeAddBuff = 0,
		AfterAddBuff = 1,
		BeforeRemoveBuff = 2,
		AfterRemoveBuff = 3,
		BuffTimeout = 4,
		LogicRecvClientMessage = 5,
		LogicRecvRequestMessage = 6,
		GateRecvClientMessage = 7,
		GateRecvServerMessage = 8,
	}
}