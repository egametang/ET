using System;
using System.Collections.Generic;
using YIUIFramework;
using Logger = YIUIFramework.Logger;

namespace YIUIFramework
{
    public class UIEventP2<P1, P2> : UIEventBase, IUIEventInvoke<P1, P2>
    {
        private LinkedList<UIEventHandleP2<P1, P2>> m_UIEventDelegates;
        public  LinkedList<UIEventHandleP2<P1, P2>> UIEventDelegates => m_UIEventDelegates;

        public UIEventP2()
        {
        }

        public UIEventP2(string name) : base(name)
        {
        }

        public void Invoke(P1 p1, P2 p2)
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
                    value.UIEventParamDelegate?.Invoke(p1, p2);
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
                PublicUIEventP2<P1, P2>.HandlerPool.Release(first.Value);
                first = m_UIEventDelegates.First;
            }

            LinkedListPool<UIEventHandleP2<P1, P2>>.Release(m_UIEventDelegates);
            m_UIEventDelegates = null;
            return true;
        }

        public UIEventHandleP2<P1, P2> Add(UIEventDelegate<P1, P2> callback)
        {
            m_UIEventDelegates ??= LinkedListPool<UIEventHandleP2<P1, P2>>.Get();

            if (callback == null)
            {
                Logger.LogError($"{EventName} 添加了一个空回调");
            }

            var handler = PublicUIEventP2<P1, P2>.HandlerPool.Get();
            var node    = m_UIEventDelegates.AddLast(handler);
            return handler.Init(m_UIEventDelegates, node, callback);
        }

        public bool Remove(UIEventHandleP2<P1, P2> handle)
        {
            m_UIEventDelegates ??= LinkedListPool<UIEventHandleP2<P1, P2>>.Get();

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
            return $"UIEventP2<{GetParamTypeString(0)},{GetParamTypeString(1)}>";
        }

        public override string GetEventHandleType()
        {
            return $"UIEventHandleP2<{GetParamTypeString(0)},{GetParamTypeString(1)}>";
        }
        #endif
    }
}