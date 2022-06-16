namespace ET.Server
{
    // 离开视野
    [Event(SceneType.Map)]
    public class UnitLeaveSightRange_NotifyClient: AEvent<AOIEntity, EventType.UnitLeaveSightRange>
    {
        protected override async ETTask Run(AOIEntity a, EventType.UnitLeaveSightRange args)
        {
            await ETTask.CompletedTask;
            AOIEntity b = args.B;
            if (a.Unit.Type != UnitType.Player)
            {
                return;
            }

            UnitHelper.NoticeUnitRemove(a.GetParent<Unit>(), b.GetParent<Unit>());
        }
    }
}