using System;
using UnityEngine;

namespace ET
{
	[UIEvent(UIType.UIHelp)]
    public class UIHelpEvent: AUIEvent
    {
        public override async ETTask<UI> OnCreate(UIComponent uiComponent)
        {
	        try
	        {
		        await uiComponent.Domain.GetComponent<ResourcesLoaderComponent>().LoadAsync(UIType.UIHelp.StringToAB());
		        GameObject bundleGameObject = (GameObject) ResourcesComponent.Instance.GetAsset(UIType.UIHelp.StringToAB(), UIType.UIHelp);
		        GameObject gameObject = UnityEngine.Object.Instantiate(bundleGameObject);
		        UI ui = uiComponent.AddChild<UI, string, GameObject>(UIType.UIHelp, gameObject);

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
        }
    }
}