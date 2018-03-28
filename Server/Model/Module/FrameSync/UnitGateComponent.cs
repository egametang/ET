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
		public long GateSessionId;

		public void Awake(long gateSessionId)
		{
			this.GateSessionId = gateSessionId;
		}

		public ActorProxy GetActorProxy()
		{
			return Game.Scene.GetComponent<ActorProxyComponent>().Get(this.GateSessionId);
		}
	}
}