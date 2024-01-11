using System.Collections.Generic;
using YIUIFramework;

namespace YIUIFramework
{
    /// <summary>
    /// UI事件全局对象池
    /// </summary>
    public static class PublicUIEventP0
    {
        public static readonly ObjectPool<UIEventHandleP0> HandlerPool = new ObjectPool<UIEventHandleP0>(
            null, handler => handler.Dispose());
    }

    /// <summary>
    /// UI事件 无参数
    /// </summary>
    public sealed class UIEventHandleP0
    {
        private LinkedList<UIEventHandleP0>     m_UIEventList;
        private LinkedListNode<UIEventHandleP0> m_UIEventNode;

        private UIEventDelegate m_UIEventParamDelegate;
        public  UIEventDelegate UIEventParamDelegate => m_UIEventParamDelegate;

        public UIEventHandleP0()
        {
        }

        internal UIEventHandleP0 Init(
            LinkedList<UIEventHandleP0>     uiEventList,
            LinkedListNode<UIEventHandleP0> uiEventNode,
            UIEventDelegate                 uiEventDelegate)
        {
            m_UIEventList          = uiEventList;
            m_UIEventNode          = uiEventNode;
            m_UIEventParamDelegate = uiEventDelegate;
            return this;
        }

        public void Dispose()
        {
            if (m_UIEventList == null || m_UIEventNode == null) return;

            m_UIEventList.Remove(m_UIEventNode);
            m_UIEventNode = null;
            m_UIEventList = null;
        }
    }
}