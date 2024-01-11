using System;
using System.Collections.Generic;
using ET;

namespace YIUIFramework
{
    public class UITaskEventP1<P1>: UIEventBase, IUITaskEventInvoke<P1>
    {
        private LinkedList<UITaskEventHandleP1<P1>> m_UITaskEventDelegates;
        public  LinkedList<UITaskEventHandleP1<P1>> UITaskEventDelegates => m_UITaskEventDelegates;

        public UITaskEventP1()
        {
        }

        public UITaskEventP1(string name): base(name)
        {
        }

        public async ETTask Invoke(P1 p1)
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
                    list.Add(value.UITaskEventParamDelegate.Invoke(p1));
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
                PublicUITaskEventP1<P1>.HandlerPool.Release(first.Value);
                first = m_UITaskEventDelegates.First;
            }

            LinkedListPool<UITaskEventHandleP1<P1>>.Release(m_UITaskEventDelegates);
            m_UITaskEventDelegates = null;
            return true;
        }

        public UITaskEventHandleP1<P1> Add(UITaskEventDelegate<P1> callback)
        {
            m_UITaskEventDelegates ??= LinkedListPool<UITaskEventHandleP1<P1>>.Get();

            if (callback == null)
            {
                Logger.LogError($"{EventName} 添加了一个空回调");
            }

            var handler = PublicUITaskEventP1<P1>.HandlerPool.Get();
            var node    = m_UITaskEventDelegates.AddLast(handler);
            return handler.Init(m_UITaskEventDelegates, node, callback);
        }

        public bool Remove(UITaskEventHandleP1<P1> handle)
        {
            m_UITaskEventDelegates ??= LinkedListPool<UITaskEventHandleP1<P1>>.Get();

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
            return $"UITaskEventP1<{GetParamTypeString(0)}>";
        }

        public override string GetEventHandleType()
        {
            return $"UITaskEventHandleP1<{GetParamTypeString(0)}>";
        }
        #endif
    }
}