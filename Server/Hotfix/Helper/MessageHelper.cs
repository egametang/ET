using Model;

namespace Hotfix
{
	public static class MessageHelper
	{
		public static void Broadcast(IMessage message)
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
