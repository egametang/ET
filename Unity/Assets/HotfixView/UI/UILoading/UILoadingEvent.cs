using System;
using UnityEngine;

namespace ET
{
	[UIEvent(UIType.UILoading)]
    public class UILoadingEvent: AUIEvent
    {
        public override async ETTask<UI> OnCreate(UIComponent uiComponent)
        {
	        try
	        {
		        await ETTask.CompletedTask;
				GameObject bundleGameObject = ((GameObject)Resources.Load("KV")).Get<GameObject>(UIType.UILoading);
				GameObject go = UnityEngine.Object.Instantiate(bundleGameObject);
				go.layer = LayerMask.NameToLayer(LayerNames.UI);
				UI ui = EntityFactory.CreateWithParent<UI, string, GameObject>(uiComponent, UIType.UILoading, go);

				ui.AddComponent<UILoadingComponent>();
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