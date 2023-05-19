using UnityEngine;

namespace ET.Client
{
    [UIEvent(UIType.UILobby)]
    public class UILobbyEvent: AUIEvent
    {
        public override async ETTask<UI> OnCreate(UIComponent uiComponent, UILayer uiLayer)
        {
            GameObject gameObject = await ResComponent.Instance.LoadAssetAsync<GameObject>(ResPathHelper.GetUIPath(UIType.UILobby));
            var go = GameObject.Instantiate(gameObject,UIEventComponent.Instance.GetLayer((int)uiLayer));
            UI ui = uiComponent.AddChild<UI, string, GameObject>(UIType.UILobby, go);

            ui.AddComponent<UILobbyComponent>();
            return ui;
        }

        public override void OnRemove(UIComponent uiComponent)
        {
            ResComponent.Instance.UnloadAsset(ResPathHelper.GetUIPath(UIType.UILobby));
        }
    }
}