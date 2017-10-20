using System;
using Model;
using UnityEngine;

namespace Hotfix
{
    [UIFactory((int)UIType.Login)]
    public class UILoginFactory : IUIFactory
    {
        public UI Create(Scene scene, UIType type, GameObject gameObject)
        {
	        try
	        {
				//GameObject bundleGameObject = ((GameObject)Resources.Load("UI")).Get<GameObject>("UILogin");

                GameObject bundleGameObject = Game.Scene.GetComponent<ResourcesComponent>().GetAsset<GameObject>("hall/login", "UILogin");
                GameObject lobby = UnityEngine.Object.Instantiate(bundleGameObject);
				lobby.layer = LayerMask.NameToLayer(LayerNames.UI);
				UI ui = new UI(scene, type, null, lobby);

				ui.AddComponent<UILoginComponent>();
				return ui;
	        }
	        catch (Exception e)
	        {
				Log.Error(e.ToStr());
		        return null;
	        }
		}
    }
}