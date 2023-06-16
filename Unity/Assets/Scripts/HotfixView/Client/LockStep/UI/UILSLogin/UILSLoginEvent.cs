using System;
using UnityEngine;

namespace ET.Client
{
    [UIEvent(UIType.UILSLogin)]
    public class UILSLoginEvent: AUIEvent
    {
        public override async ETTask<UI> OnCreate(UIComponent uiComponent, UILayer uiLayer)
        {
            await uiComponent.Scene().GetComponent<ResourcesLoaderComponent>().LoadAsync(UIType.UILSLogin.StringToAB());
            GameObject bundleGameObject = (GameObject) ResourcesComponent.Instance.GetAsset(UIType.UILSLogin.StringToAB(), UIType.UILSLogin);
            GameObject gameObject = UnityEngine.Object.Instantiate(bundleGameObject, UIEventComponent.Instance.GetLayer((int)uiLayer));
            UI ui = uiComponent.AddChild<UI, string, GameObject>(UIType.UILSLogin, gameObject);
            ui.AddComponent<UILSLoginComponent>();
            return ui;
        }

        public override void OnRemove(UIComponent uiComponent)
        {
            ResourcesComponent.Instance.UnloadBundle(UIType.UILSLogin.StringToAB());
        }
    }
}