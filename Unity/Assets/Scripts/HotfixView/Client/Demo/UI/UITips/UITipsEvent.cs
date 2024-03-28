/*********************************************
 * 自动生成代码，请勿手动修改
 * 脚本名：UITipsEvent.cs
 * 创建时间：2024/03/28 14:56:53
 *********************************************/
using System;
using UnityEngine;

namespace ET.Client
{
    [UIEvent(UIType.UITips)]
    public class UITipsEvent: AUIEvent
    {
        public override async ETTask<UI> OnCreate(UIComponent uiComponent, UILayer uiLayer)
        {
            string assetsName = $"Assets/Bundles/UI/Demo/{UIType.UITips}.prefab";
            GameObject bundleGameObject = await uiComponent.Scene().GetComponent<ResourcesLoaderComponent>().LoadAssetAsync<GameObject>(assetsName);
            GameObject gameObject = UnityEngine.Object.Instantiate(bundleGameObject, uiComponent.UIGlobalComponent.GetLayer((int)uiLayer));
            UI ui = uiComponent.AddChild<UI, string, GameObject>(UIType.UITips, gameObject);
            ui.AddComponent<UITipsComponent>();
            return ui;
        }

        public override void OnRemove(UIComponent uiComponent)
        {
        }
    }
}
