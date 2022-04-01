namespace ET
{
    // 离开视野
    [Event]
    public class UnitLeaveSightRange_NotifyClient: AEvent<EventType.UnitLeaveSightRange>
    {
        protected override void Run(EventType.UnitLeaveSightRange args)
        {
            AOIEntity a = args.A;
            AOIEntity b = args.B;
            if (a.Unit.Type != UnitType.Player)
            {
                return;
            }

            UnitHelper.NoticeUnitRemove(a.GetParent<Unit>(), b.GetParent<Unit>());
        }
    }
}