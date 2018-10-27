namespace ETModel
{
    [Event(EventIdType.LoadingBegin)]
    public class LoadingBeginEvent_CreateLoadingUI : AEvent
    {
        public override void Run()
        {
            // 创建FGUI检查更新面板
            FGUI.Open<UI_CheckUpdate>();
        }
    }
}
