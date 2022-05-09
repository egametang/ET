namespace ET.Server
{
    // 进入视野通知
    [Event(SceneType.Map)]
    public class UnitEnterSightRange_NotifyClient: AEvent<AOIEntity, EventType.UnitEnterSightRange>
    {
        protected override async ETTask Run(AOIEntity a, EventType.UnitEnterSightRange args)
        {
            await ETTask.CompletedTask;
            AOIEntity b = args.B;
            if (a.Id == b.Id)
            {
                return;
            }

            Unit ua = a.GetParent<Unit>();
            if (ua.Type != UnitType.Player)
            {
                return;
            }

            Unit ub = b.GetParent<Unit>();

            UnitHelper.NoticeUnitAdd(ua, ub);
        }
    }
}