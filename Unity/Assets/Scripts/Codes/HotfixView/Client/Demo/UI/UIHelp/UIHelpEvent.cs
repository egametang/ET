using System;
using UnityEngine;

namespace ET.Client
{
	[UIEvent(UIType.UIHelp)]
    public class UIHelpEvent: AUIEvent
    {
        public override async ETTask<UI> OnCreate(UIComponent uiComponent, UILayer uiLayer)
        {
	        try
	        {
		        GameObject gameObject = await ResComponent.Instance.LoadAssetAsync<GameObject>(ResPathHelper.GetUIPath(UIType.UIHelp));
		        var go = GameObject.Instantiate(gameObject,UIEventComponent.Instance.GetLayer((int)uiLayer));
		        UI ui = uiComponent.AddChild<UI, string, GameObject>(UIType.UIHelp, go);
		        ui.AddComponent<UIHelpComponent>();
				return ui;
	        }
	        catch (Exception e)
	        {
				Log.Error(e);
		        return null;
	        }
		}

        public override void OnRemove(UIComponent uiComponent)
        {
	        ResComponent.Instance.UnloadAsset(ResPathHelper.GetUIPath(UIType.UIHelp));
        }
    }
}