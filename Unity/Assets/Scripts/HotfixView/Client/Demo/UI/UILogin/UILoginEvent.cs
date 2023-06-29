using System;
using UnityEngine;

namespace ET.Client
{
    [UIEvent(UIType.UILogin)]
    public class UILoginEvent: AUIEvent
    {
        public override async ETTask<UI> OnCreate(UIComponent uiComponent, UILayer uiLayer)
        {
            ResourcesComponent resourcesComponent = uiComponent.Root().GetComponent<ResourcesComponent>();
            await uiComponent.Scene().GetComponent<ResourcesLoaderComponent>().LoadAsync(resourcesComponent.StringToAB(UIType.UILogin));
            GameObject bundleGameObject = (GameObject) resourcesComponent.GetAsset(resourcesComponent.StringToAB(UIType.UILogin), UIType.UILogin);
            GameObject gameObject = UnityEngine.Object.Instantiate(bundleGameObject, uiComponent.UIGlobalComponent.GetLayer((int)uiLayer));
            UI ui = uiComponent.AddChild<UI, string, GameObject>(UIType.UILogin, gameObject);
            ui.AddComponent<UILoginComponent>();
            return ui;
        }

        public override void OnRemove(UIComponent uiComponent)
        {
        }
    }
}