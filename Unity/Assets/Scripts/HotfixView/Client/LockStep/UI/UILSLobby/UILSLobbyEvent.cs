using UnityEngine;

namespace ET.Client
{
    [UIEvent(UIType.UILSLobby)]
    public class UILSLobbyEvent: AUIEvent
    {
        public override async ETTask<UI> OnCreate(UIComponent uiComponent, UILayer uiLayer)
        {
            await ETTask.CompletedTask;
            ResourcesComponent resourcesComponent = uiComponent.Root().GetComponent<ResourcesComponent>();
            await uiComponent.Scene().GetComponent<ResourcesLoaderComponent>().LoadAsync(resourcesComponent.StringToAB(UIType.UILSLobby));
            GameObject bundleGameObject = (GameObject) resourcesComponent.GetAsset(resourcesComponent.StringToAB(UIType.UILSLobby), UIType.UILSLobby);
            GameObject gameObject = UnityEngine.Object.Instantiate(bundleGameObject, uiComponent.UIGlobalComponent.GetLayer((int)uiLayer));
            UI ui = uiComponent.AddChild<UI, string, GameObject>(UIType.UILSLobby, gameObject);

            ui.AddComponent<UILSLobbyComponent>();
            return ui;
        }

        public override void OnRemove(UIComponent uiComponent)
        {
        }
    }
}