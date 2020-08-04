namespace ET
{
    public static class UIHelper
    {
        public static async ETTask<UI> Create(Scene scene, string uiType)
        {
            UIComponent uiComponent = scene.GetComponent<UIComponent>();
            return await UIEventComponent.Instance.OnCreate(uiComponent, uiType);
        }
        
        public static async ETTask Remove(Scene scene, string uiType)
        {
            UIComponent uiComponent = scene.GetComponent<UIComponent>();
            UIEventComponent.Instance.OnRemove(uiComponent, uiType);
        }
    }
}