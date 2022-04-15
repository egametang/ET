namespace ET.Server
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
		public long GateSessionActorId { get; private set; }

		public void Awake(long gateSessionId)
		{
			this.GateSessionActorId = gateSessionId;
		}
	}
}