using System;
using UnityEngine;

namespace ET.Client
{
    [EntitySystemOf(typeof(UIGlobalComponent))]
    public static partial class UIGlobalComponentSystem
    {
        [EntitySystem]
        public static void Awake(this UIGlobalComponent self)
        {
            GameObject uiRoot = GameObject.Find("/Global/UI");
            ReferenceCollector referenceCollector = uiRoot.GetComponent<ReferenceCollector>();

            self.UILayers.Add((int)UILayer.Hidden, referenceCollector.Get<GameObject>(UILayer.Hidden.ToString()).transform);
            self.UILayers.Add((int)UILayer.Low, referenceCollector.Get<GameObject>(UILayer.Low.ToString()).transform);
            self.UILayers.Add((int)UILayer.Mid, referenceCollector.Get<GameObject>(UILayer.Mid.ToString()).transform);
            self.UILayers.Add((int)UILayer.High, referenceCollector.Get<GameObject>(UILayer.High.ToString()).transform);
        }

        public static async ETTask<UI> OnCreate(this UIGlobalComponent self, UIComponent uiComponent, string uiType, UILayer uiLayer)
        {
            try
            {
                UI ui = await UIEventComponent.Instance.UIEvents[uiType].OnCreate(uiComponent, uiLayer);
                return ui;
            }
            catch (Exception e)
            {
                throw new Exception($"on create ui error: {uiType}", e);
            }
        }

        public static Transform GetLayer(this UIGlobalComponent self, int layer)
        {
            return self.UILayers[layer];
        }

        public static void OnRemove(this UIGlobalComponent self, UIComponent uiComponent, string uiType)
        {
            try
            {
                UIEventComponent.Instance.UIEvents[uiType].OnRemove(uiComponent);
            }
            catch (Exception e)
            {
                throw new Exception($"on remove ui error: {uiType}", e);
            }
        }
    }
}