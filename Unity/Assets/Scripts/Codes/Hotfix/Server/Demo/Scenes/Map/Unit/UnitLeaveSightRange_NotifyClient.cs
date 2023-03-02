namespace ET.Server
{
    // 离开视野
    [Event(SceneType.Map)]
    public class UnitLeaveSightRange_NotifyClient: AEvent<EventType.UnitLeaveSightRange>
    {
        protected override async ETTask Run(Scene scene, EventType.UnitLeaveSightRange args)
        {
            AOIEntity a = args.A;
            AOIEntity b = args.B;
            if (a.Id == b.Id)
            {
                return;
            }
            
            Unit ua = a.Unit;
            if (ua.Type != UnitType.Player)
            {
                return;
            }

            MessageHelper.NoticeUnitRemove(ua, b.Unit);
            
            await ETTask.CompletedTask;
        }
    }
}