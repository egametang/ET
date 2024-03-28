/*********************************************
 * 
 * 脚本名：UIRegisterEvent.cs
 * 创建时间：2024/03/28 11:40:37
 *********************************************/
using System;
using UnityEngine;

namespace ET.Client
{
    [UIEvent(UIType.UIRegister)]
    public class UIRegisterEvent: AUIEvent
    {
        public override async ETTask<UI> OnCreate(UIComponent uiComponent, UILayer uiLayer)
        {
            string assetsName = $"Assets/Bundles/UI/Demo/{UIType.UIRegister}.prefab";
            GameObject bundleGameObject = await uiComponent.Scene().GetComponent<ResourcesLoaderComponent>().LoadAssetAsync<GameObject>(assetsName);
            GameObject gameObject = UnityEngine.Object.Instantiate(bundleGameObject, uiComponent.UIGlobalComponent.GetLayer((int)uiLayer));
            UI ui = uiComponent.AddChild<UI, string, GameObject>(UIType.UIRegister, gameObject);
            ui.AddComponent<UIRegisterComponent>();
            return ui;
        }

        public override void OnRemove(UIComponent uiComponent)
        {
        }
    }
}
