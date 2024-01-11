using System;
using System.Collections.Generic;
using YIUIFramework;
using Logger = YIUIFramework.Logger;

namespace YIUIFramework
{
    public class UIEventP0: UIEventBase, IUIEventInvoke
    {
        private LinkedList<UIEventHandleP0> m_UIEventDelegates;
        public  LinkedList<UIEventHandleP0> UIEventDelegates => m_UIEventDelegates;

        public UIEventP0()
        {
        }

        public UIEventP0(string name): base(name)
        {
        }

        public void Invoke()
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
                    value.UIEventParamDelegate?.Invoke();
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
                PublicUIEventP0.HandlerPool.Release(first.Value);
                first = m_UIEventDelegates.First;
            }

            LinkedListPool<UIEventHandleP0>.Release(m_UIEventDelegates);
            m_UIEventDelegates = null;
            return true;
        }

        public UIEventHandleP0 Add(UIEventDelegate callback)
        {
            m_UIEventDelegates ??= LinkedListPool<UIEventHandleP0>.Get();

            if (callback == null)
            {
                Logger.LogError($"{EventName} 添加了一个空回调");
            }

            var handler = PublicUIEventP0.HandlerPool.Get();
            var node    = m_UIEventDelegates.AddLast(handler);
            return handler.Init(m_UIEventDelegates, node, callback);
        }

        public bool Remove(UIEventHandleP0 handle)
        {
            m_UIEventDelegates ??= LinkedListPool<UIEventHandleP0>.Get();

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
            return "UIEventP0";
        }

        public override string GetEventHandleType()
        {
            return "UIEventHandleP0";
        }
        #endif
    }
}