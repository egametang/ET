using System.Collections.Generic;

namespace YIUIFramework
{
    /// <summary>
    /// UI事件全局对象池
    /// </summary>
    public static class PublicUITaskEventP5<P1, P2, P3, P4, P5>
    {
        public static readonly ObjectPool<UITaskEventHandleP5<P1, P2, P3, P4, P5>> HandlerPool =
            new ObjectPool<UITaskEventHandleP5<P1, P2, P3, P4, P5>>(
                null, handler => handler.Dispose());
    }

    /// <summary>
    /// UI事件 5个泛型参数
    /// </summary>
    public sealed class UITaskEventHandleP5<P1, P2, P3, P4, P5>
    {
        private LinkedList<UITaskEventHandleP5<P1, P2, P3, P4, P5>>     m_UITaskEventList;
        private LinkedListNode<UITaskEventHandleP5<P1, P2, P3, P4, P5>> m_UITaskEventNode;

        private UITaskEventDelegate<P1, P2, P3, P4, P5> m_UITaskEventParamDelegate;
        public  UITaskEventDelegate<P1, P2, P3, P4, P5> UITaskEventParamDelegate => m_UITaskEventParamDelegate;

        public UITaskEventHandleP5()
        {
        }

        internal UITaskEventHandleP5<P1, P2, P3, P4, P5> Init(
            LinkedList<UITaskEventHandleP5<P1, P2, P3, P4, P5>>     uiTaskEventList,
            LinkedListNode<UITaskEventHandleP5<P1, P2, P3, P4, P5>> uiTaskEventNode,
            UITaskEventDelegate<P1, P2, P3, P4, P5>                 uiTaskEventDelegate)
        {
            m_UITaskEventList          = uiTaskEventList;
            m_UITaskEventNode          = uiTaskEventNode;
            m_UITaskEventParamDelegate = uiTaskEventDelegate;
            return this;
        }

        public void Dispose()
        {
            if (m_UITaskEventList == null || m_UITaskEventNode == null) return;

            m_UITaskEventList.Remove(m_UITaskEventNode);
            m_UITaskEventNode = null;
            m_UITaskEventList = null;
        }
    }
}