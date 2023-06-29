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
		        ResourcesComponent resourcesComponent = uiComponent.Root().GetComponent<ResourcesComponent>();
		        await uiComponent.Scene().GetComponent<ResourcesLoaderComponent>().LoadAsync(resourcesComponent.StringToAB(UIType.UIHelp));
		        GameObject bundleGameObject = (GameObject) resourcesComponent.GetAsset(resourcesComponent.StringToAB(UIType.UIHelp), UIType.UIHelp);
		        GameObject gameObject = UnityEngine.Object.Instantiate(bundleGameObject, uiComponent.UIGlobalComponent.GetLayer((int)uiLayer));
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