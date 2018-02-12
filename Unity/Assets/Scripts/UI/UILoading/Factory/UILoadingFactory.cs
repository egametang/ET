using System;
using UnityEngine;

namespace Model
{
    [UIFactory((int)UIType.UILoading)]
    public class UILoadingFactory : IUIFactory
    {
        public UI Create(Scene scene, UIType type, GameObject gameObject)
        {
	        try
	        {
				GameObject bundleGameObject = ((GameObject)Resources.Load("KV")).Get<GameObject>("UILoading");
				GameObject go = UnityEngine.Object.Instantiate(bundleGameObject);
				go.layer = LayerMask.NameToLayer(LayerNames.UI);
				UI ui = ComponentFactory.Create<UI, Scene, UI, GameObject>(scene, null, go);

				ui.AddComponent<UILoadingComponent>();
				return ui;
	        }
	        catch (Exception e)
	        {
				Log.Error(e.ToString());
		        return null;
	        }
		}

	    public void Remove(UIType type)
	    {
	    }
    }
}