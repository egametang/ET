namespace ET
{
	public class EnterMapFinish_RemoveLobbyUI: AEvent<EventType.EnterMapFinish>
	{
		public override async ETTask Run(EventType.EnterMapFinish args)
		{
			await UIHelper.Remove(args.ZoneScene, UIType.UILobby);
		}
	}
}
