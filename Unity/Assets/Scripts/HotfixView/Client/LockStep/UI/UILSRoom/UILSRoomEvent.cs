using System;
using UnityEngine;

namespace ET.Client
{
    [UIEvent(UIType.UILSRoom)]
    public class UILSRoomEvent: AUIEvent
    {
        public override async ETTask<UI> OnCreate(UIComponent uiComponent, UILayer uiLayer)
        {
            ResourcesComponent resourcesComponent = uiComponent.Root().GetComponent<ResourcesComponent>();
            await uiComponent.Room().GetComponent<ResourcesLoaderComponent>().LoadAsync(resourcesComponent.StringToAB(UIType.UILSRoom));
            GameObject bundleGameObject = (GameObject) resourcesComponent.GetAsset(resourcesComponent.StringToAB(UIType.UILSRoom), UIType.UILSRoom);
            GameObject gameObject = UnityEngine.Object.Instantiate(bundleGameObject, uiComponent.UIGlobalComponent.GetLayer((int)uiLayer));
            UI ui = uiComponent.AddChild<UI, string, GameObject>(UIType.UILSRoom, gameObject);
            ui.AddComponent<UILSRoomComponent>();
            return ui;
        }

        public override void OnRemove(UIComponent uiComponent)
        {
        }
    }
}