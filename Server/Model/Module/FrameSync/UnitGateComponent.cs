namespace ETModel
{
	[ObjectSystem]
	public class UnitGateComponentAwakeSystem : AwakeSystem<UnitGateComponent, long>
	{
		public override void Awake(UnitGateComponent self, long a)
		{
			self.Awake(a);
		}
	}

	public class UnitGateComponent : Component, ISerializeToEntity
	{
		public long GateSessionActorId;

		public bool IsDisconnect;

		public void Awake(long gateSessionId)
		{
			this.GateSessionActorId = gateSessionId;
		}

		public ActorMessageSender GetActorMessageSender()
		{
			return Game.Scene.GetComponent<ActorMessageSenderComponent>().Get(this.GateSessionActorId);
		}
	}
}