using System.Collections.Generic;
using YIUIFramework;

namespace YIUIFramework
{
    /// <summary>
    /// UI事件全局对象池
    /// </summary>
    public static class PublicUIEventP2<P1, P2>
    {
        public static readonly ObjectPool<UIEventHandleP2<P1, P2>> HandlerPool =
            new ObjectPool<UIEventHandleP2<P1, P2>>(
                null, handler => handler.Dispose());
    }

    /// <summary>
    /// UI事件 2个泛型参数
    /// </summary>
    public sealed class UIEventHandleP2<P1, P2>
    {
        private LinkedList<UIEventHandleP2<P1, P2>>     m_UIEventList;
        private LinkedListNode<UIEventHandleP2<P1, P2>> m_UIEventNode;

        private UIEventDelegate<P1, P2> m_UIEventParamDelegate;
        public  UIEventDelegate<P1, P2> UIEventParamDelegate => m_UIEventParamDelegate;

        public UIEventHandleP2()
        {
        }

        internal UIEventHandleP2<P1, P2> Init(
            LinkedList<UIEventHandleP2<P1, P2>>     uiEventList,
            LinkedListNode<UIEventHandleP2<P1, P2>> uiEventNode,
            UIEventDelegate<P1, P2>                 uiEventDelegate)
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