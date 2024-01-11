using System;
using System.Collections.Generic;
using YIUIFramework;
using Logger = YIUIFramework.Logger;

namespace YIUIFramework
{
    public class UIEventP4<P1, P2, P3, P4> : UIEventBase, IUIEventInvoke<P1, P2, P3, P4>
    {
        private LinkedList<UIEventHandleP4<P1, P2, P3, P4>> m_UIEventDelegates;
        public  LinkedList<UIEventHandleP4<P1, P2, P3, P4>> UIEventDelegates => m_UIEventDelegates;

        public UIEventP4()
        {
        }

        public UIEventP4(string name) : base(name)
        {
        }

        public void Invoke(P1 p1, P2 p2, P3 p3, P4 p4)
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
                    value.UIEventParamDelegate?.Invoke(p1, p2, p3, p4);
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
                PublicUIEventP4<P1, P2, P3, P4>.HandlerPool.Release(first.Value);
                first = m_UIEventDelegates.First;
            }

            LinkedListPool<UIEventHandleP4<P1, P2, P3, P4>>.Release(m_UIEventDelegates);
            m_UIEventDelegates = null;
            return true;
        }

        public UIEventHandleP4<P1, P2, P3, P4> Add(UIEventDelegate<P1, P2, P3, P4> callback)
        {
            m_UIEventDelegates ??= LinkedListPool<UIEventHandleP4<P1, P2, P3, P4>>.Get();

            if (callback == null)
            {
                Logger.LogError($"{EventName} 添加了一个空回调");
            }

            var handler = PublicUIEventP4<P1, P2, P3, P4>.HandlerPool.Get();
            var node    = m_UIEventDelegates.AddLast(handler);
            return handler.Init(m_UIEventDelegates, node, callback);
        }

        public bool Remove(UIEventHandleP4<P1, P2, P3, P4> handle)
        {
            m_UIEventDelegates ??= LinkedListPool<UIEventHandleP4<P1, P2, P3, P4>>.Get();

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
                $"UIEventP4<{GetParamTypeString(0)},{GetParamTypeString(1)},{GetParamTypeString(2)},{GetParamTypeString(3)}>";
        }

        public override string GetEventHandleType()
        {
            return
                $"UIEventHandleP4<{GetParamTypeString(0)},{GetParamTypeString(1)},{GetParamTypeString(2)},{GetParamTypeString(3)}>";
        }
        #endif
    }
}