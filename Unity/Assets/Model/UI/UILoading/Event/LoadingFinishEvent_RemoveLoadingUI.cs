namespace ETModel
{
    [Event(EventIdType.LoadingFinish)]
    public class LoadingFinishEvent_RemoveLoadingUI : AEvent
    {
        public override void Run()
        {
            FGUI.Close(typeof(UI_CheckUpdate));
        }
    }
}
