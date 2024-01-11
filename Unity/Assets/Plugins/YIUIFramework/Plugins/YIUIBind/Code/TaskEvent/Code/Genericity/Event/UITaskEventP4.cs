using System;
using System.Collections.Generic;
using ET;

namespace YIUIFramework
{
    public class UITaskEventP4<P1, P2, P3, P4>: UIEventBase, IUITaskEventInvoke<P1, P2, P3, P4>
    {
        private LinkedList<UITaskEventHandleP4<P1, P2, P3, P4>> m_UITaskEventDelegates;
        public  LinkedList<UITaskEventHandleP4<P1, P2, P3, P4>> UITaskEventDelegates => m_UITaskEventDelegates;

        public UITaskEventP4()
        {
        }

        public UITaskEventP4(string name): base(name)
        {
        }

        public async ETTask Invoke(P1 p1, P2 p2, P3 p3, P4 p4)
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
                    list.Add(value.UITaskEventParamDelegate.Invoke(p1, p2, p3, p4));
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
                PublicUITaskEventP4<P1, P2, P3, P4>.HandlerPool.Release(first.Value);
                first = m_UITaskEventDelegates.First;
            }

            LinkedListPool<UITaskEventHandleP4<P1, P2, P3, P4>>.Release(m_UITaskEventDelegates);
            m_UITaskEventDelegates = null;
            return true;
        }

        public UITaskEventHandleP4<P1, P2, P3, P4> Add(UITaskEventDelegate<P1, P2, P3, P4> callback)
        {
            m_UITaskEventDelegates ??= LinkedListPool<UITaskEventHandleP4<P1, P2, P3, P4>>.Get();

            if (callback == null)
            {
                Logger.LogError($"{EventName} 添加了一个空回调");
            }

            var handler = PublicUITaskEventP4<P1, P2, P3, P4>.HandlerPool.Get();
            var node    = m_UITaskEventDelegates.AddLast(handler);
            return handler.Init(m_UITaskEventDelegates, node, callback);
        }

        public bool Remove(UITaskEventHandleP4<P1, P2, P3, P4> handle)
        {
            m_UITaskEventDelegates ??= LinkedListPool<UITaskEventHandleP4<P1, P2, P3, P4>>.Get();

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
            return
                    $"UITaskEventP4<{GetParamTypeString(0)},{GetParamTypeString(1)},{GetParamTypeString(2)},{GetParamTypeString(3)}>";
        }

        public override string GetEventHandleType()
        {
            return
                    $"UITaskEventHandleP4<{GetParamTypeString(0)},{GetParamTypeString(1)},{GetParamTypeString(2)},{GetParamTypeString(3)}>";
        }
        #endif
    }
}