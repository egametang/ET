using System;
using System.Collections.Generic;
using YIUIFramework;
using Logger = YIUIFramework.Logger;

namespace YIUIFramework
{
    public class UIEventP5<P1, P2, P3, P4, P5> : UIEventBase, IUIEventInvoke<P1, P2, P3, P4, P5>
    {
        private LinkedList<UIEventHandleP5<P1, P2, P3, P4, P5>> m_UIEventDelegates;
        public  LinkedList<UIEventHandleP5<P1, P2, P3, P4, P5>> UIEventDelegates => m_UIEventDelegates;

        public UIEventP5()
        {
        }

        public UIEventP5(string name) : base(name)
        {
        }

        public void Invoke(P1 p1, P2 p2, P3 p3, P4 p4, P5 p5)
        {
            if (m_UIEventDelegates == null)
            {
                Logger.LogWarning($"{EventName} 未绑定任何事件");
                return;
            }

            var itr = m_UIEventDelegates.First;
            while (itr != null)
            {
                var next  = itr.Next;
                var value = itr.Value;
                try
                {
                    value.UIEventParamDelegate?.Invoke(p1, p2, p3, p4, p5);
                }
                catch (Exception e)
                {
                    Logger.LogError(e);
                }

                itr = next;
            }
        }

        public override bool IsTaskEvent => false;
        
        public override bool Clear()
        {
            if (m_UIEventDelegates == null) return false;

            var first = m_UIEventDelegates.First;
            while (first != null)
            {
                PublicUIEventP5<P1, P2, P3, P4, P5>.HandlerPool.Release(first.Value);
                first = m_UIEventDelegates.First;
            }

            LinkedListPool<UIEventHandleP5<P1, P2, P3, P4, P5>>.Release(m_UIEventDelegates);
            m_UIEventDelegates = null;
            return true;
        }

        public UIEventHandleP5<P1, P2, P3, P4, P5> Add(UIEventDelegate<P1, P2, P3, P4, P5> callback)
        {
            m_UIEventDelegates ??= LinkedListPool<UIEventHandleP5<P1, P2, P3, P4, P5>>.Get();

            if (callback == null)
            {
                Logger.LogError($"{EventName} 添加了一个空回调");
            }

            var handler = PublicUIEventP5<P1, P2, P3, P4, P5>.HandlerPool.Get();
            var node    = m_UIEventDelegates.AddLast(handler);
            return handler.Init(m_UIEventDelegates, node, callback);
        }

        public bool Remove(UIEventHandleP5<P1, P2, P3, P4, P5> handle)
        {
            m_UIEventDelegates ??= LinkedListPool<UIEventHandleP5<P1, P2, P3, P4, P5>>.Get();

            if (handle == null)
            {
                Logger.LogError($"{EventName} UIEventParamHandle == null");
                return false;
            }

            return m_UIEventDelegates.Remove(handle);
        }
        #if UNITY_EDITOR
        public override string GetEventType()
        {
            return
                $"UIEventP5<{GetParamTypeString(0)},{GetParamTypeString(1)},{GetParamTypeString(2)},{GetParamTypeString(3)},{GetParamTypeString(4)}>";
        }

        public override string GetEventHandleType()
        {
            return
                $"UIEventHandleP5<{GetParamTypeString(0)},{GetParamTypeString(1)},{GetParamTypeString(2)},{GetParamTypeString(3)},{GetParamTypeString(4)}>";
        }
        #endif
    }
}