using System;
using UnityEngine;

namespace ETModel
{
    [UIFactory(UIType.UILoading)]
    public class UILoadingFactory : IUIFactory
    {
        public UI Create(Scene scene, string type, GameObject gameObject)
        {
	        try
	        {
				GameObject bundleGameObject = ((GameObject)ResourcesHelper.Load("KV")).Get<GameObject>(type);
				GameObject go = UnityEngine.Object.Instantiate(bundleGameObject);
				go.layer = LayerMask.NameToLayer(LayerNames.UI);
				UI ui = ComponentFactory.Create<UI, GameObject>(go);

				ui.AddComponent<UILoadingComponent>();
				return ui;
	        }
	        catch (Exception e)
	        {
				Log.Error(e);
		        return null;
	        }
		}

	    public void Remove(string type)
	    {
	    }
    }
}