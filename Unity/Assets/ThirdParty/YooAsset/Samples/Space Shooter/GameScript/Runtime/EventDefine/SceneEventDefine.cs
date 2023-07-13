using UniFramework.Event;

public class SceneEventDefine
{
	public class ChangeToHomeScene : IEventMessage
	{
		public static void SendEventMessage()
		{
			var msg = new ChangeToHomeScene();
			UniEvent.SendMessage(msg);
		}
	}

	public class ChangeToBattleScene : IEventMessage
	{
		public static void SendEventMessage()
		{
			var msg = new ChangeToBattleScene();
			UniEvent.SendMessage(msg);
		}
	}
}