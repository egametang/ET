using System;
using UnityEngine;

namespace ET
{
    [UIEvent(UIType.UILobby)]
    public class UILobbyEvent: AUIEvent
    {
        public override async ETTask<UI> OnCreate(UIComponent uiComponent, UILayer uiLayer)
        {
            await ETTask.CompletedTask;
            await uiComponent.Domain.GetComponent<ResourcesLoaderComponent>().LoadAsync(UIType.UILobby.StringToAB());
            GameObject bundleGameObject = (GameObject) ResourcesComponent.Instance.GetAsset(UIType.UILobby.StringToAB(), UIType.UILobby);
            GameObject gameObject = UnityEngine.Object.Instantiate(bundleGameObject, UIEventComponent.Instance.UILayers[(int)uiLayer]);
            UI ui = uiComponent.AddChild<UI, string, GameObject>(UIType.UILobby, gameObject);

            ui.AddComponent<UILobbyComponent>();
            return ui;
        }

        public override void OnRemove(UIComponent uiComponent)
        {
            ResourcesComponent.Instance.UnloadBundle(UIType.UILobby.StringToAB());
        }
    }
}