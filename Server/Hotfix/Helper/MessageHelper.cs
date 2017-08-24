using Model;

namespace Hotfix
{
	public static class MessageHelper
	{
		public static void Broadcast<Message>(Message message) where Message: AMessage
		{
			Gamer[] gamers = Game.Scene.GetComponent<GamerComponent>().GetAll();
			foreach (Gamer gamer in gamers)
			{
				gamer.GetComponent<SessionInfoComponent>().Session.Send(message);
			}
		}
	}
}
