namespace ET
{
    public static class UIHelper
    {
        public static async ETTask<UI> Create(Scene scene, string uiType, UILayer uiLayer)
        {
            UI ui = await scene.GetComponent<UIComponent>().Create(uiType, uiLayer);
            return ui;
        }
        
        public static async ETTask Remove(Scene scene, string uiType)
        {
            scene.GetComponent<UIComponent>().Remove(uiType);
            await ETTask.CompletedTask;
        }
    }
}