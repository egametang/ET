using UnityEngine;

namespace ET.Client
{
    [UIEvent(UIType.UILSLobby)]
    public class UILSLobbyEvent: AUIEvent
    {
        public override async ETTask<UI> OnCreate(UIComponent uiComponent, UILayer uiLayer)
        {
            await ETTask.CompletedTask;
            await uiComponent.Scene().GetComponent<ResourcesLoaderComponent>().LoadAsync(UIType.UILSLobby.StringToAB());
            GameObject bundleGameObject = (GameObject) ResourcesComponent.Instance.GetAsset(UIType.UILSLobby.StringToAB(), UIType.UILSLobby);
            GameObject gameObject = UnityEngine.Object.Instantiate(bundleGameObject, UIEventComponent.Instance.GetLayer((int)uiLayer));
            UI ui = uiComponent.AddChild<UI, string, GameObject>(UIType.UILSLobby, gameObject);

            ui.AddComponent<UILSLobbyComponent>();
            return ui;
        }

        public override void OnRemove(UIComponent uiComponent)
        {
            ResourcesComponent.Instance.UnloadBundle(UIType.UILSLobby.StringToAB());
        }
    }
}