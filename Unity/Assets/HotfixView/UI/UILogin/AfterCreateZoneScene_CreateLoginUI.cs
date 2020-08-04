

namespace ET
{
	public class AfterCreateZoneScene_RemoveLoginUI: AEvent<EventType.AfterCreateZoneScene>
	{
		public override async ETTask Run(EventType.AfterCreateZoneScene args)
		{
			await UIHelper.Create(args.ZoneScene, UIType.UILogin);
		}
	}
}
