using System;
using UnityEngine;

namespace ET
{
	[UIEvent(UIType.UILobby)]
    public class UILobbyEvent: AUIEvent
    {
        public override async ETTask<UI> OnCreate(UIComponent uiComponent)
        {
	        try
	        {
				ResourcesComponent resourcesComponent = Game.Scene.GetComponent<ResourcesComponent>();
		        resourcesComponent.LoadBundle(UIType.UILobby.StringToAB());
				GameObject bundleGameObject = (GameObject)resourcesComponent.GetAsset(UIType.UILobby.StringToAB(), UIType.UILobby);
				GameObject gameObject = UnityEngine.Object.Instantiate(bundleGameObject);
		        UI ui = EntityFactory.Create<UI, string, GameObject>(uiComponent.Domain, UIType.UILobby, gameObject);
		        
		        ui.AddComponent<UILobbyComponent>();
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
	        uiComponent.Remove(UIType.UILobby);
        }
    }
}