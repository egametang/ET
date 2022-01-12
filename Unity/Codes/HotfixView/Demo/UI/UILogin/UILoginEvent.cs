﻿using System;
using UnityEngine;

namespace ET
{
    [UIEvent(UIType.UILogin)]
    public class UILoginEvent: AUIEvent
    {
        public override async ETTask<UI> OnCreate(UIComponent uiComponent)
        {
            await uiComponent.Parent.GetComponent<ResourcesLoaderComponent>().LoadAsync(UIType.UILogin.StringToAB());
            GameObject bundleGameObject = (GameObject) ResourcesComponent.Instance.GetAsset(UIType.UILogin.StringToAB(), UIType.UILogin);
            GameObject gameObject = UnityEngine.Object.Instantiate(bundleGameObject);
            UI ui = uiComponent.AddChild<UI, string, GameObject>(UIType.UILogin, gameObject);
            ui.AddComponent<UILoginComponent>();
            return ui;
        }

        public override void OnRemove(UIComponent uiComponent)
        {
            ResourcesComponent.Instance.UnloadBundle(UIType.UILogin.StringToAB());
        }
    }
}