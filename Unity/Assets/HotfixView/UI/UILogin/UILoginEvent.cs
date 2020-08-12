using System;
using UnityEngine;

namespace ET
{
    [UIEvent(UIType.UILogin)]
    public class UILoginEvent: AUIEvent
    {
        public override async ETTask<UI> OnCreate(UIComponent uiComponent)
        {
            ResourcesComponent resourcesComponent = Game.Scene.GetComponent<ResourcesComponent>();
            await resourcesComponent.LoadBundleAsync(UIType.UILogin.StringToAB());
            GameObject bundleGameObject = (GameObject) resourcesComponent.GetAsset(UIType.UILogin.StringToAB(), UIType.UILogin);
            GameObject gameObject = UnityEngine.Object.Instantiate(bundleGameObject);

            UI ui = EntityFactory.CreateWithParent<UI, string, GameObject>(uiComponent, UIType.UILogin, gameObject);

            ui.AddComponent<UILoginComponent>();
            return ui;
        }

        public override void OnRemove(UIComponent uiComponent)
        {
            uiComponent.Remove(UIType.UILogin);
        }
    }
}