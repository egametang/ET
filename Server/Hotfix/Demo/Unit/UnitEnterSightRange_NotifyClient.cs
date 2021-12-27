namespace ET
{
    // 进入视野通知
    [Event]
    public class UnitEnterSightRange_NotifyClient: AEvent<EventType.UnitEnterSightRange>
    {
        protected override async ETTask Run(EventType.UnitEnterSightRange args)
        {
            await ETTask.CompletedTask;
            AOIEntity a = args.A;
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