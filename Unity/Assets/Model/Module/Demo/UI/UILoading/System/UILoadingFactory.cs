using System;
using UnityEngine;

namespace ETModel
{
    public static class UILoadingFactory
    {
        public static UI Create(Entity domain)
        {
	        try
	        {
				GameObject bundleGameObject = ((GameObject)ResourcesHelper.Load("KV")).Get<GameObject>(UIType.UILoading);
				GameObject go = UnityEngine.Object.Instantiate(bundleGameObject);
				go.layer = LayerMask.NameToLayer(LayerNames.UI);
				UI ui = EntityFactory.Create<UI, string, GameObject>(domain, UIType.UILoading, go);

				ui.AddComponent<UILoadingComponent>();
				return ui;
	        }
	        catch (Exception e)
	        {
				Log.Error(e);
		        return null;
	        }
		}

	    public static void Remove(string type)
	    {
	    }
    }
}