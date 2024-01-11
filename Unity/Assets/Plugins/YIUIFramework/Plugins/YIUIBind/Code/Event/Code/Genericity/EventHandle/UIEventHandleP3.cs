using System.Collections.Generic;
using YIUIFramework;

namespace YIUIFramework
{
    /// <summary>
    /// UI事件全局对象池
    /// </summary>
    public static class PublicUIEventP3<P1, P2, P3>
    {
        public static readonly ObjectPool<UIEventHandleP3<P1, P2, P3>> HandlerPool =
            new ObjectPool<UIEventHandleP3<P1, P2, P3>>(
                null, handler => handler.Dispose());
    }

    /// <summary>
    /// UI事件 3个泛型参数
    /// </summary>
    public sealed class UIEventHandleP3<P1, P2, P3>
    {
        private LinkedList<UIEventHandleP3<P1, P2, P3>>     m_UIEventList;
        private LinkedListNode<UIEventHandleP3<P1, P2, P3>> m_UIEventNode;

        private UIEventDelegate<P1, P2, P3> m_UIEventParamDelegate;
        public  UIEventDelegate<P1, P2, P3> UIEventParamDelegate => m_UIEventParamDelegate;

        public UIEventHandleP3()
        {
        }

        internal UIEventHandleP3<P1, P2, P3> Init(
            LinkedList<UIEventHandleP3<P1, P2, P3>>     uiEventList,
            LinkedListNode<UIEventHandleP3<P1, P2, P3>> uiEventNode,
            UIEventDelegate<P1, P2, P3>                 uiEventDelegate)
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