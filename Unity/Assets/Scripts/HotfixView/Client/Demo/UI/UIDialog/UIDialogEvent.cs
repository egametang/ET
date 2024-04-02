/*********************************************
 * 自动生成代码，请勿手动修改
 * 脚本名：UIDialogEvent.cs
 * 创建时间：2024/04/01 11:51:43
 *********************************************/
using System;
using UnityEngine;

namespace ET.Client
{
    [UIEvent(UIType.UIDialog)]
    public class UIDialogEvent: AUIEvent
    {
        public override async ETTask<UI> OnCreate(UIComponent uiComponent, UILayer uiLayer)
        {
            string assetsName = $"Assets/Bundles/UI/Demo/{UIType.UIDialog}.prefab";
            GameObject bundleGameObject = await uiComponent.Scene().GetComponent<ResourcesLoaderComponent>().LoadAssetAsync<GameObject>(assetsName);
            GameObject gameObject = UnityEngine.Object.Instantiate(bundleGameObject, uiComponent.UIGlobalComponent.GetLayer((int)uiLayer));
            UI ui = uiComponent.AddChild<UI, string, GameObject>(UIType.UIDialog, gameObject);
            ui.AddComponent<UIDialogComponent>();
            return ui;
        }

        public override void OnRemove(UIComponent uiComponent)
        {
        }
    }
}
