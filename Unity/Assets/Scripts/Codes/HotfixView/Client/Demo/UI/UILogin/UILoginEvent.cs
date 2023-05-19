using System;
using UnityEngine;

namespace ET.Client
{
    [UIEvent(UIType.UILogin)]
    public class UILoginEvent: AUIEvent
    {
        public override async ETTask<UI> OnCreate(UIComponent uiComponent, UILayer uiLayer)
        {
            GameObject gameObject = await ResComponent.Instance.LoadAssetAsync<GameObject>(ResPathHelper.GetUIPath(UIType.UILogin));
            var go = GameObject.Instantiate(gameObject,UIEventComponent.Instance.GetLayer((int)uiLayer));
            UI ui = uiComponent.AddChild<UI, string, GameObject>(UIType.UILogin, go);
            ui.AddComponent<UILoginComponent>();
            return ui;
        }

        public override void OnRemove(UIComponent uiComponent)
        {
            ResComponent.Instance.UnloadAsset(ResPathHelper.GetUIPath(UIType.UILogin));
        }
    }
}