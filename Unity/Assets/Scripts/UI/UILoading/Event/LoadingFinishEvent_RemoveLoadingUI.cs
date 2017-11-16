namespace Model
{
    [Event((int)EventIdType.LoadingFinish)]
    public class LoadingFinishEvent_RemoveLoadingUI : IEvent
    {
        public void Run()
        {
			Game.Scene.GetComponent<UIComponent>().Remove(UIType.UILoading);
        }
    }
}
