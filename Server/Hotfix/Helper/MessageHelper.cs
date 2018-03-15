using ETModel;

namespace ETHotfix
{
	public static class MessageHelper
	{
		public static void Broadcast(IActorMessage message)
		{
			Unit[] units = Game.Scene.GetComponent<UnitComponent>().GetAll();
			ActorProxyComponent actorProxyComponent = Game.Scene.GetComponent<ActorProxyComponent>();
			foreach (Unit unit in units)
			{
				long gateSessionId = unit.GetComponent<UnitGateComponent>().GateSessionId;
				actorProxyComponent.Get(gateSessionId).Send(message);
			}
		}
	}
}
