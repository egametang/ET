using UnityEngine;

namespace ET.Client
{
    [UIEvent(UIType.UILSLobby)]
    public class UILSLobbyEvent: AUIEvent
    {
        public override async ETTask<UI> OnCreate(UIComponent uiComponent, UILayer uiLayer)
        {
            string assetsName = $"Assets/Bundles/UI/LockStep/{UIType.UILSLobby}.prefab";
            GameObject bundleGameObject = await uiComponent.Scene().GetComponent<ResourcesLoaderComponent>().LoadAssetAsync<GameObject>(assetsName);
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