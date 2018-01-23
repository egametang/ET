namespace Model
{
	[ObjectSystem]
	public class UnitGateComponentSystem : ObjectSystem<UnitGateComponent>, IAwake<long>
	{
		public void Awake(long gateSessionId)
		{
			this.Get().Awake(gateSessionId);
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