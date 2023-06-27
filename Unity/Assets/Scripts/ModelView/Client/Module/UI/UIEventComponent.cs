using System;
using System.Collections.Generic;
using UnityEngine;

namespace ET.Client
{
	/// <summary>
	/// 管理所有UI GameObject
	/// </summary>
	public class UIEventComponent: Singleton<UIEventComponent>, ISingletonAwake
	{
		public Dictionary<string, AUIEvent> UIEvents = new Dictionary<string, AUIEvent>();
		
		public Dictionary<int, Transform> UILayers = new Dictionary<int, Transform>();
		
        public void Awake()
        {
            GameObject uiRoot = GameObject.Find("/Global/UI");
            ReferenceCollector referenceCollector = uiRoot.GetComponent<ReferenceCollector>();

            this.UILayers.Add((int)UILayer.Hidden, referenceCollector.Get<GameObject>(UILayer.Hidden.ToString()).transform);
            this.UILayers.Add((int)UILayer.Low, referenceCollector.Get<GameObject>(UILayer.Low.ToString()).transform);
            this.UILayers.Add((int)UILayer.Mid, referenceCollector.Get<GameObject>(UILayer.Mid.ToString()).transform);
            this.UILayers.Add((int)UILayer.High, referenceCollector.Get<GameObject>(UILayer.High.ToString()).transform);

            var uiEvents = EventSystem.Instance.GetTypes(typeof (UIEventAttribute));
            foreach (Type type in uiEvents)
            {
                object[] attrs = type.GetCustomAttributes(typeof (UIEventAttribute), false);
                if (attrs.Length == 0)
                {
                    continue;
                }

                UIEventAttribute uiEventAttribute = attrs[0] as UIEventAttribute;
                AUIEvent aUIEvent = Activator.CreateInstance(type) as AUIEvent;
                this.UIEvents.Add(uiEventAttribute.UIType, aUIEvent);
            }
        }

        public async ETTask<UI> OnCreate(UIComponent uiComponent, string uiType, UILayer uiLayer)
        {
            try
            {
                UI ui = await this.UIEvents[uiType].OnCreate(uiComponent, uiLayer);
                return ui;
            }
            catch (Exception e)
            {
                throw new Exception($"on create ui error: {uiType}", e);
            }
        }

        public Transform GetLayer(int layer)
        {
            return this.UILayers[layer];
        }

        public void OnRemove(UIComponent uiComponent, string uiType)
        {
            try
            {
                this.UIEvents[uiType].OnRemove(uiComponent);
            }
            catch (Exception e)
            {
                throw new Exception($"on remove ui error: {uiType}", e);
            }
        }
	}
}