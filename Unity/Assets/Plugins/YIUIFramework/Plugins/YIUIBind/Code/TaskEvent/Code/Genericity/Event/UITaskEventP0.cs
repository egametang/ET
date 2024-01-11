using System;
using System.Collections.Generic;
using ET;

namespace YIUIFramework
{
    public class UITaskEventP0: UIEventBase, IUITaskEventInvoke
    {
        private LinkedList<UITaskEventHandleP0> m_UITaskEventDelegates;
        public  LinkedList<UITaskEventHandleP0> UITaskEventDelegates => m_UITaskEventDelegates;

        public UITaskEventP0()
        {
        }

        public UITaskEventP0(string name): base(name)
        {
        }

        public async ETTask Invoke()
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
                    list.Add(value.UITaskEventParamDelegate.Invoke());
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
                PublicUITaskEventP0.HandlerPool.Release(first.Value);
                first = m_UITaskEventDelegates.First;
            }

            LinkedListPool<UITaskEventHandleP0>.Release(m_UITaskEventDelegates);
            m_UITaskEventDelegates = null;
            return true;
        }

        public UITaskEventHandleP0 Add(UITaskEventDelegate callback)
        {
            m_UITaskEventDelegates ??= LinkedListPool<UITaskEventHandleP0>.Get();

            if (callback == null)
            {
                Logger.LogError($"{EventName} 添加了一个空回调");
            }

            var handler = PublicUITaskEventP0.HandlerPool.Get();
            var node    = m_UITaskEventDelegates.AddLast(handler);
            return handler.Init(m_UITaskEventDelegates, node, callback);
        }

        public bool Remove(UITaskEventHandleP0 handle)
        {
            m_UITaskEventDelegates ??= LinkedListPool<UITaskEventHandleP0>.Get();

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
            return "UITaskEventP0";
        }

        public override string GetEventHandleType()
        {
            return "UITaskEventHandleP0";
        }
        #endif
    }
}