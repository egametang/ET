using System.Collections.Generic;
using YIUIFramework;

namespace YIUIFramework
{
    /// <summary>
    /// UI事件全局对象池
    /// </summary>
    public static class PublicUIEventP5<P1, P2, P3, P4, P5>
    {
        public static readonly ObjectPool<UIEventHandleP5<P1, P2, P3, P4, P5>> HandlerPool =
            new ObjectPool<UIEventHandleP5<P1, P2, P3, P4, P5>>(
                null, handler => handler.Dispose());
    }

    /// <summary>
    /// UI事件 5个泛型参数
    /// </summary>
    public sealed class UIEventHandleP5<P1, P2, P3, P4, P5>
    {
        private LinkedList<UIEventHandleP5<P1, P2, P3, P4, P5>>     m_UIEventList;
        private LinkedListNode<UIEventHandleP5<P1, P2, P3, P4, P5>> m_UIEventNode;

        private UIEventDelegate<P1, P2, P3, P4, P5> m_UIEventParamDelegate;
        public  UIEventDelegate<P1, P2, P3, P4, P5> UIEventParamDelegate => m_UIEventParamDelegate;

        public UIEventHandleP5()
        {
        }

        internal UIEventHandleP5<P1, P2, P3, P4, P5> Init(
            LinkedList<UIEventHandleP5<P1, P2, P3, P4, P5>>     uiEventList,
            LinkedListNode<UIEventHandleP5<P1, P2, P3, P4, P5>> uiEventNode,
            UIEventDelegate<P1, P2, P3, P4, P5>                 uiEventDelegate)
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