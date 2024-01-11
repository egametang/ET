using System.Collections.Generic;
using YIUIFramework;

namespace YIUIFramework
{
    /// <summary>
    /// UI事件全局对象池
    /// </summary>
    public static class PublicUIEventP4<P1, P2, P3, P4>
    {
        public static readonly ObjectPool<UIEventHandleP4<P1, P2, P3, P4>> HandlerPool =
            new ObjectPool<UIEventHandleP4<P1, P2, P3, P4>>(
                null, handler => handler.Dispose());
    }

    /// <summary>
    /// UI事件 4个泛型参数
    /// </summary>
    public sealed class UIEventHandleP4<P1, P2, P3, P4>
    {
        private LinkedList<UIEventHandleP4<P1, P2, P3, P4>>     m_UIEventList;
        private LinkedListNode<UIEventHandleP4<P1, P2, P3, P4>> m_UIEventNode;

        private UIEventDelegate<P1, P2, P3, P4> m_UIEventParamDelegate;
        public  UIEventDelegate<P1, P2, P3, P4> UIEventParamDelegate => m_UIEventParamDelegate;

        public UIEventHandleP4()
        {
        }

        internal UIEventHandleP4<P1, P2, P3, P4> Init(
            LinkedList<UIEventHandleP4<P1, P2, P3, P4>>     uiEventList,
            LinkedListNode<UIEventHandleP4<P1, P2, P3, P4>> uiEventNode,
            UIEventDelegate<P1, P2, P3, P4>                 uiEventDelegate)
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