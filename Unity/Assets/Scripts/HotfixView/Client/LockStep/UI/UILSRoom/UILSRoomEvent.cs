using System;
using UnityEngine;

namespace ET.Client
{
    [UIEvent(UIType.UILSRoom)]
    public class UILSRoomEvent: AUIEvent
    {
        public override async ETTask<UI> OnCreate(UIComponent uiComponent, UILayer uiLayer)
        {
            await uiComponent.Room().GetComponent<ResourcesLoaderComponent>().LoadAsync(UIType.UILSRoom.StringToAB());
            GameObject bundleGameObject = (GameObject) ResourcesComponent.Instance.GetAsset(UIType.UILSRoom.StringToAB(), UIType.UILSRoom);
            GameObject gameObject = UnityEngine.Object.Instantiate(bundleGameObject, UIEventComponent.Instance.GetLayer((int)uiLayer));
            UI ui = uiComponent.AddChild<UI, string, GameObject>(UIType.UILSRoom, gameObject);
            ui.AddComponent<UILSRoomComponent>();
            return ui;
        }

        public override void OnRemove(UIComponent uiComponent)
        {
            ResourcesComponent.Instance.UnloadBundle(UIType.UILSRoom.StringToAB());
        }
    }
}