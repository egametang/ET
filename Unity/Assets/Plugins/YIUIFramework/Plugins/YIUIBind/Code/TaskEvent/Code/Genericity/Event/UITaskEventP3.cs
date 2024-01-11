using System;
using System.Collections.Generic;
using ET;

namespace YIUIFramework
{
    public class UITaskEventP3<P1, P2, P3>: UIEventBase, IUITaskEventInvoke<P1, P2, P3>
    {
        private LinkedList<UITaskEventHandleP3<P1, P2, P3>> m_UITaskEventDelegates;
        public  LinkedList<UITaskEventHandleP3<P1, P2, P3>> UITaskEventDelegates => m_UITaskEventDelegates;

        public UITaskEventP3()
        {
        }

        public UITaskEventP3(string name): base(name)
        {
        }

        public async ETTask Invoke(P1 p1, P2 p2, P3 p3)
        {
            if (m_UITaskEventDelegates == null)
            {
                Logger.LogWarning($"{EventName} 未绑定任何事件");
                return;
            }

            using var list = ListComponent<ETTask>.Create();

            var itr = m_UITaskEventDelegates.First;
            while (itr != null)
            {
                var next  = itr.Next;
                var value = itr.Value;
                if (value.UITaskEventParamDelegate != null)
                    list.Add(value.UITaskEventParamDelegate.Invoke(p1, p2, p3));
                itr = next;
            }

            try
            {
                await ETTaskHelper.WaitAll(list);
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

        public override bool IsTaskEvent => true;

        public override bool Clear()
        {
            if (m_UITaskEventDelegates == null) return false;

            var first = m_UITaskEventDelegates.First;
            while (first != null)
            {
                PublicUITaskEventP3<P1, P2, P3>.HandlerPool.Release(first.Value);
                first = m_UITaskEventDelegates.First;
            }

            LinkedListPool<UITaskEventHandleP3<P1, P2, P3>>.Release(m_UITaskEventDelegates);
            m_UITaskEventDelegates = null;
            return true;
        }

        public UITaskEventHandleP3<P1, P2, P3> Add(UITaskEventDelegate<P1, P2, P3> callback)
        {
            m_UITaskEventDelegates ??= LinkedListPool<UITaskEventHandleP3<P1, P2, P3>>.Get();

            if (callback == null)
            {
                Logger.LogError($"{EventName} 添加了一个空回调");
            }

            var handler = PublicUITaskEventP3<P1, P2, P3>.HandlerPool.Get();
            var node    = m_UITaskEventDelegates.AddLast(handler);
            return handler.Init(m_UITaskEventDelegates, node, callback);
        }

        public bool Remove(UITaskEventHandleP3<P1, P2, P3> handle)
        {
            m_UITaskEventDelegates ??= LinkedListPool<UITaskEventHandleP3<P1, P2, P3>>.Get();

            if (handle == null)
            {
                Logger.LogError($"{EventName} UITaskEventParamHandle == null");
                return false;
            }

            return m_UITaskEventDelegates.Remove(handle);
        }
        #if UNITY_EDITOR
        public override string GetEventType()
        {
            return $"UITaskEventP3<{GetParamTypeString(0)},{GetParamTypeString(1)},{GetParamTypeString(2)}>";
        }

        public override string GetEventHandleType()
        {
            return $"UITaskEventHandleP3<{GetParamTypeString(0)},{GetParamTypeString(1)},{GetParamTypeString(2)}>";
        }
        #endif
    }
}