namespace ET
{
    public abstract class AUIEvent
    {
        public abstract ETTask<UI> OnCreate(UIComponent uiComponent);
        public abstract void OnRemove(UIComponent uiComponent);
    }
}