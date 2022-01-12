namespace ET
{
	public class UnitGateComponentAwakeSystem : AwakeSystem<UnitGateComponent, long>
	{
		public override void Awake(UnitGateComponent self, long a)
		{
			self.Awake(a);
		}
	}

	public class UnitGateComponent : Entity, IAwake<long>, ITransfer
	{
		public long GateSessionActorId;

		public void Awake(long gateSessionId)
		{
			this.GateSessionActorId = gateSessionId;
		}
	}
}