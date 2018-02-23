using System;
using Model;
using UnityEngine;

namespace Hotfix
{
    [UIFactory((int)UIType.UILogin)]
    public class UILoginFactory : IUIFactory
    {
        public UI Create(Scene scene, UIType type, GameObject gameObject)
        {
	        try
			{
				ResourcesComponent resourcesComponent = Game.Scene.GetComponent<ResourcesComponent>();
				resourcesComponent.LoadBundle($"{type}.unity3d");
				GameObject bundleGameObject = resourcesComponent.GetAsset<GameObject>($"{type}.unity3d", $"{type}");
				GameObject login = UnityEngine.Object.Instantiate(bundleGameObject);
				login.layer = LayerMask.NameToLayer(LayerNames.UI);
		        UI ui = ComponentFactory.Create<UI, GameObject>(login);

				ui.AddComponent<UILoginComponent>();
				return ui;
	        }
	        catch (Exception e)
	        {
				Log.Error(e.ToStr());
		        return null;
	        }
		}

	    public void Remove(UIType type)
	    {
			Game.Scene.GetComponent<ResourcesComponent>().UnloadBundle($"{type}.unity3d");
	    }
    }
}