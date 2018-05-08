using ETModel;

namespace ETHotfix
{
	public static class MessageHelper
	{
		public static void Broadcast(IActorMessage message)
		{
			Unit[] units = Game.Scene.GetComponent<UnitComponent>().GetAll();
			ActorMessageSenderComponent actorMessageSenderComponent = Game.Scene.GetComponent<ActorMessageSenderComponent>();
			foreach (Unit unit in units)
			{
				long gateSessionActorId = unit.GetComponent<UnitGateComponent>().GateSessionActorId;
				actorMessageSenderComponent.GetWithActorId(gateSessionActorId).Send(message);
			}
		}
	}
}
