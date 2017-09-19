namespace Model
{
	[ObjectEvent]
	public class UnitGateComponentEvent : ObjectEvent<UnitGateComponent>, IAwake<long>
	{
		public void Awake(long gateSessionId)
		{
			this.Get().Awake(gateSessionId);
		}
	}

	public class UnitGateComponent : ComponentDB
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