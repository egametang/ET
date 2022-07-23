using System;
using System.Collections.Generic;

namespace FairyGUI
{
    public delegate void EventCallback0();
    public delegate void EventCallback1(EventContext context);

    /// <summary>
    /// 
    /// </summary>
    public class EventDispatcher : IEventDispatcher
    {
        Dictionary<string, EventBridge> _dic;

        public EventDispatcher()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="strType"></param>
        /// <param name="callback"></param>
        public void AddEventListener(string strType, EventCallback1 callback)
        {
            GetBridge(strType).Add(callback);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="strType"></param>
        /// <param name="callback"></param>
        public void AddEventListener(string strType, EventCallback0 callback)
        {
            GetBridge(strType).Add(callback);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="strType"></param>
        /// <param name="callback"></param>
        public void RemoveEventListener(string strType, EventCallback1 callback)
        {
            if (_dic == null)
                return;

            EventBridge bridge = null;
            if (_dic.TryGetValue(strType, out bridge))
                bridge.Remove(callback);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="strType"></param>
        /// <param name="callback"></param>
        public void RemoveEventListener(string strType, EventCallback0 callback)
        {
            if (_dic == null)
                return;

            EventBridge bridge = null;
            if (_dic.TryGetValue(strType, out bridge))
                bridge.Remove(callback);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="strType"></param>
        /// <param name="callback"></param>
        public void AddCapture(string strType, EventCallback1 callback)
        {
            GetBridge(strType).AddCapture(callback);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="strType"></param>
        /// <param name="callback"></param>
        public void RemoveCapture(string strType, EventCallback1 callback)
        {
            if (_dic == null)
                return;

            EventBridge bridge = null;
            if (_dic.TryGetValue(strType, out bridge))
                bridge.RemoveCapture(callback);
        }

        /// <summary>
        /// 
        /// </summary>
        public void RemoveEventListeners()
        {
            RemoveEventListeners(null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="strType"></param>
        public void RemoveEventListeners(string strType)
        {
            if (_dic == null)
                return;

            if (strType != null)
            {
                EventBridge bridge;
                if (_dic.TryGetValue(strType, out bridge))
                    bridge.Clear();
            }
            else
            {
                foreach (KeyValuePair<string, EventBridge> kv in _dic)
                    kv.Value.Clear();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="strType"></param>
        /// <returns></returns>
        public bool hasEventListeners(string strType)
        {
            EventBridge bridge = TryGetEventBridge(strType);
            if (bridge == null)
                return false;

            return !bridge.isEmpty;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="strType"></param>
        /// <returns></returns>
        public bool isDispatching(string strType)
        {
            EventBridge bridge = TryGetEventBridge(strType);
            if (bridge == null)
                return false;

            return bridge._dispatching;
        }

        internal EventBridge TryGetEventBridge(string strType)
        {
            if (_dic == null)
                return null;

            EventBridge bridge = null;
            _dic.TryGetValue(strType, out bridge);
            return bridge;
        }

        internal EventBridge GetEventBridge(string strType)
        {
            if (_dic == null)
                _dic = new Dictionary<string, EventBridge>();

            EventBridge bridge = null;
            if (!_dic.TryGetValue(strType, out bridge))
            {
                bridge = new EventBridge(this);
                _dic[strType] = bridge;
            }
            return bridge;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="strType"></param>
        /// <returns></returns>
        public bool DispatchEvent(string strType)
        {
            return DispatchEvent(strType, null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="strType"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public bool DispatchEvent(string strType, object data)
        {
            return InternalDispatchEvent(strType, null, data, null);
        }

        public bool DispatchEvent(string strType, object data, object initiator)
        {
            return InternalDispatchEvent(strType, null, data, initiator);
        }

        static InputEvent sCurrentInputEvent = new InputEvent();

        internal bool InternalDispatchEvent(string strType, EventBridge bridge, object data, object initiator)
        {
            if (bridge == null)
                bridge = TryGetEventBridge(strType);

            EventBridge gBridge = null;
            if ((this is DisplayObject) && ((DisplayObject)this).gOwner != null)
                gBridge = ((DisplayObject)this).gOwner.TryGetEventBridge(strType);

            bool b1 = bridge != null && !bridge.isEmpty;
            bool b2 = gBridge != null && !gBridge.isEmpty;
            if (b1 || b2)
            {
                EventContext context = EventContext.Get();
                context.initiator = initiator != null ? initiator : this;
                context.type = strType;
                context.data = data;
                if (data is InputEvent)
                    sCurrentInputEvent = (InputEvent)data;
                context.inputEvent = sCurrentInputEvent;

                if (b1)
                {
                    bridge.CallCaptureInternal(context);
                    bridge.CallInternal(context);
                }

                if (b2)
                {
                    gBridge.CallCaptureInternal(context);
                    gBridge.CallInternal(context);
                }

                EventContext.Return(context);
                context.initiator = null;
                context.sender = null;
                context.data = null;

                return context._defaultPrevented;
            }
            else
                return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public bool DispatchEvent(EventContext context)
        {
            EventBridge bridge = TryGetEventBridge(context.type);
            EventBridge gBridge = null;
            if ((this is DisplayObject) && ((DisplayObject)this).gOwner != null)
                gBridge = ((DisplayObject)this).gOwner.TryGetEventBridge(context.type);

            EventDispatcher savedSender = context.sender;

            if (bridge != null && !bridge.isEmpty)
            {
                bridge.CallCaptureInternal(context);
                bridge.CallInternal(context);
            }

            if (gBridge != null && !gBridge.isEmpty)
            {
                gBridge.CallCaptureInternal(context);
                gBridge.CallInternal(context);
            }

            context.sender = savedSender;
            return context._defaultPrevented;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="strType"></param>
        /// <param name="data"></param>
        /// <param name="addChain"></param>
        /// <returns></returns>
        internal bool BubbleEvent(string strType, object data, List<EventBridge> addChain)
        {
            EventContext context = EventContext.Get();
            context.initiator = this;

            context.type = strType;
            context.data = data;
            if (data is InputEvent)
                sCurrentInputEvent = (InputEvent)data;
            context.inputEvent = sCurrentInputEvent;
            List<EventBridge> bubbleChain = context.callChain;
            bubbleChain.Clear();

            GetChainBridges(strType, bubbleChain, true);

            int length = bubbleChain.Count;
            for (int i = length - 1; i >= 0; i--)
            {
                bubbleChain[i].CallCaptureInternal(context);
                if (context._touchCapture)
                {
                    context._touchCapture = false;
                    if (strType == "onTouchBegin")
                        Stage.inst.AddTouchMonitor(context.inputEvent.touchId, bubbleChain[i].owner);
                }
            }

            if (!context._stopsPropagation)
            {
                for (int i = 0; i < length; ++i)
                {
                    bubbleChain[i].CallInternal(context);

                    if (context._touchCapture)
                    {
                        context._touchCapture = false;
                        if (strType == "onTouchBegin")
                            Stage.inst.AddTouchMonitor(context.inputEvent.touchId, bubbleChain[i].owner);
                    }

                    if (context._stopsPropagation)
                        break;
                }

                if (addChain != null)
                {
                    length = addChain.Count;
                    for (int i = 0; i < length; ++i)
                    {
                        EventBridge bridge = addChain[i];
                        if (bubbleChain.IndexOf(bridge) == -1)
                        {
                            bridge.CallCaptureInternal(context);
                            bridge.CallInternal(context);
                        }
                    }
                }
            }

            EventContext.Return(context);
            context.initiator = null;
            context.sender = null;
            context.data = null;
            return context._defaultPrevented;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="strType"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public bool BubbleEvent(string strType, object data)
        {
            return BubbleEvent(strType, data, null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="strType"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public bool BroadcastEvent(string strType, object data)
        {
            EventContext context = EventContext.Get();
            context.initiator = this;
            context.type = strType;
            context.data = data;
            if (data is InputEvent)
                sCurrentInputEvent = (InputEvent)data;
            context.inputEvent = sCurrentInputEvent;
            List<EventBridge> bubbleChain = context.callChain;
            bubbleChain.Clear();

            if (this is Container)
                GetChildEventBridges(strType, (Container)this, bubbleChain);
            else if (this is GComponent)
                GetChildEventBridges(strType, (GComponent)this, bubbleChain);

            int length = bubbleChain.Count;
            for (int i = 0; i < length; ++i)
                bubbleChain[i].CallInternal(context);

            EventContext.Return(context);
            context.initiator = null;
            context.sender = null;
            context.data = null;
            return context._defaultPrevented;
        }

        EventBridge GetBridge(string strType)
        {
            if (strType == null)
                throw new Exception("event type cant be null");

            if (_dic == null)
                _dic = new Dictionary<string, EventBridge>();

            EventBridge bridge = null;
            if (!_dic.TryGetValue(strType, out bridge))
            {
                bridge = new EventBridge(this);
                _dic[strType] = bridge;
            }

            return bridge;
        }

        static void GetChildEventBridges(string strType, Container container, List<EventBridge> bridges)
        {
            EventBridge bridge = container.TryGetEventBridge(strType);
            if (bridge != null)
                bridges.Add(bridge);
            if (container.gOwner != null)
            {
                bridge = container.gOwner.TryGetEventBridge(strType);
                if (bridge != null && !bridge.isEmpty)
                    bridges.Add(bridge);
            }

            int count = container.numChildren;
            for (int i = 0; i < count; ++i)
            {
                DisplayObject obj = container.GetChildAt(i);
                if (obj is Container)
                    GetChildEventBridges(strType, (Container)obj, bridges);
                else
                {
                    bridge = obj.TryGetEventBridge(strType);
                    if (bridge != null && !bridge.isEmpty)
                        bridges.Add(bridge);

                    if (obj.gOwner != null)
                    {
                        bridge = obj.gOwner.TryGetEventBridge(strType);
                        if (bridge != null && !bridge.isEmpty)
                            bridges.Add(bridge);
                    }
                }
            }
        }

        static void GetChildEventBridges(string strType, GComponent container, List<EventBridge> bridges)
        {
            EventBridge bridge = container.TryGetEventBridge(strType);
            if (bridge != null)
                bridges.Add(bridge);

            int count = container.numChildren;
            for (int i = 0; i < count; ++i)
            {
                GObject obj = container.GetChildAt(i);
                if (obj is GComponent)
                    GetChildEventBridges(strType, (GComponent)obj, bridges);
                else
                {
                    bridge = obj.TryGetEventBridge(strType);
                    if (bridge != null)
                        bridges.Add(bridge);
                }
            }
        }

        internal void GetChainBridges(string strType, List<EventBridge> chain, bool bubble)
        {
            EventBridge bridge = TryGetEventBridge(strType);
            if (bridge != null && !bridge.isEmpty)
                chain.Add(bridge);

            if ((this is DisplayObject) && ((DisplayObject)this).gOwner != null)
            {
                bridge = ((DisplayObject)this).gOwner.TryGetEventBridge(strType);
                if (bridge != null && !bridge.isEmpty)
                    chain.Add(bridge);
            }

            if (!bubble)
                return;

            if (this is DisplayObject)
            {
                DisplayObject element = (DisplayObject)this;
                while ((element = element.parent) != null)
                {
                    bridge = element.TryGetEventBridge(strType);
                    if (bridge != null && !bridge.isEmpty)
                        chain.Add(bridge);

                    if (element.gOwner != null)
                    {
                        bridge = element.gOwner.TryGetEventBridge(strType);
                        if (bridge != null && !bridge.isEmpty)
                            chain.Add(bridge);
                    }
                }
            }
            else if (this is GObject)
            {
                GObject element = (GObject)this;
                while ((element = element.parent) != null)
                {
                    bridge = element.TryGetEventBridge(strType);
                    if (bridge != null && !bridge.isEmpty)
                        chain.Add(bridge);
                }
            }
        }
    }
}
