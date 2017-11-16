namespace Model
{
    [Event((int)EventIdType.LoadingBegin)]
    public class LoadingBeginEvent_CreateLoadingUI : IEvent
    {
        public void Run()
        {
			Game.Scene.GetComponent<UIComponent>().Create(UIType.UILoading);
        }
    }
}
