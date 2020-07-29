namespace ET
{
    public class LoadingFinishEvent_RemoveLoadingUI : AEvent<EventType.LoadingFinish>
    {
        public override async ETTask Run(EventType.LoadingFinish args)
        {
            args.Scene.GetComponent<UIComponent>().Remove(UIType.UILoading);
        }
    }
}
