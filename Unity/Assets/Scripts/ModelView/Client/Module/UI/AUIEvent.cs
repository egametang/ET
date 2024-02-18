namespace ET.Client
{
    public abstract class AUIEvent: HandlerObject
    {
        public abstract ETTask<UI> OnCreate(UIComponent uiComponent, UILayer uiLayer);
        public abstract void OnRemove(UIComponent uiComponent);
    }
}