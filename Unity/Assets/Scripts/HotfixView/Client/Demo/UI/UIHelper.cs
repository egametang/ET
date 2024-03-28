namespace ET.Client
{
    public static class UIHelper
    {
        [EnableAccessEntiyChild]
        public static async ETTask<UI> Create(Entity scene, string uiType, UILayer uiLayer)
        {
            return await scene.GetComponent<UIComponent>().Create(uiType, uiLayer);
        }

        [EnableAccessEntiyChild]
        public static async ETTask Hide(Entity scene, string uiType)
        {
            scene.GetComponent<UIComponent>().Hide(uiType); 
            await ETTask.CompletedTask;
        }

        [EnableAccessEntiyChild]
        public static async ETTask Remove(Entity scene, string uiType)
        {
            scene.GetComponent<UIComponent>().Remove(uiType);
            await ETTask.CompletedTask;
        }
        
        [EnableAccessEntiyChild]
        public static async ETTask<UI> Get(Entity scene, string uiType)
        {
            return scene.GetComponent<UIComponent>().Get(uiType);
        }

        [EnableAccessEntiyChild]
        public static async ETTask ShowTips(Entity scene, string tips = "")
        {
            UI ui = await Create(scene, UIType.UITips,UILayer.High);
            var uiTipsComponent = ui.GetComponent<UITipsComponent>();
            await uiTipsComponent.Log(tips);
            await Hide(scene, UIType.UITips);
        }

    }
}