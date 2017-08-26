using Model;

namespace Hotfix
{
	public static class MessageHelper
	{
		public static void Broadcast<Message>(Message message) where Message: AMessage
		{
			Player[] players = Game.Scene.GetComponent<PlayerComponent>().GetAll();
			foreach (Player gamer in players)
			{
				gamer.GetComponent<SessionInfoComponent>().Session.Send(message);
			}
		}
	}
}
