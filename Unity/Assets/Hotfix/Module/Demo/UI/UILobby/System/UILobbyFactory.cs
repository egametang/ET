using System;
using ETModel;
using UnityEngine;

namespace ETHotfix
{
    public static class UILobbyFactory
    {
        public static UI Create()
        {
	        try
	        {
				ResourcesComponent resourcesComponent = ETModel.Game.Scene.GetComponent<ResourcesComponent>();
		        resourcesComponent.LoadBundle(UIType.UILobby.StringToAB());
				GameObject bundleGameObject = (GameObject)resourcesComponent.GetAsset(UIType.UILobby.StringToAB(), UIType.UILobby);
				GameObject gameObject = UnityEngine.Object.Instantiate(bundleGameObject);
		        UI ui = ComponentFactory.Create<UI, string, GameObject>(UIType.UILobby, gameObject, false);

				ui.AddComponent<UILobbyComponent>();
				return ui;
	        }
	        catch (Exception e)
	        {
				Log.Error(e);
		        return null;
	        }
		}
    }
}