namespace ET
{
    [Event]
    public class LoadingFinishEvent_RemoveLoadingUI : AEvent<EventType.LoadingFinish>
    {
        public override void Run(EventType.LoadingFinish args)
        {
			Game.Scene.GetComponent<UIComponent>().Remove(UIType.UILoading);
        }
    }
}
