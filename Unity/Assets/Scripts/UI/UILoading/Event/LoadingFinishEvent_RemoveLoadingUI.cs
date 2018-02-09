namespace Model
{
    [Event(EventIdType.LoadingFinish)]
    public class LoadingFinishEvent_RemoveLoadingUI : AEvent
    {
        public override void Run()
        {
			Game.Scene.GetComponent<UIComponent>().Remove(UIType.UILoading);
        }
    }
}
