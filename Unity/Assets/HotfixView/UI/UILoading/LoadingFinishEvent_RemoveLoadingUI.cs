namespace ET
{
    public class LoadingFinishEvent_RemoveLoadingUI : AEvent<EventType.LoadingFinish>
    {
        public override async ETTask Run(EventType.LoadingFinish args)
        {
            await UIHelper.Create(args.Scene, UIType.UILoading);
        }
    }
}
