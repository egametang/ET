namespace ETModel
{
    [Event(EventIdType.LoadingBegin)]
    public class LoadingBeginEvent_CreateLoadingUI : AEvent
    {
        public override void Run()
        {
			Game.Scene.GetComponent<UIComponent>().Create(UIType.UILoading);
        }
    }
}
