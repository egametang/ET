using System.Collections.Generic;

namespace YIUIFramework
{
    /// <summary>
    /// UI事件全局对象池
    /// </summary>
    public static class PublicUITaskEventP4<P1, P2, P3, P4>
    {
        public static readonly ObjectPool<UITaskEventHandleP4<P1, P2, P3, P4>> HandlerPool =
            new ObjectPool<UITaskEventHandleP4<P1, P2, P3, P4>>(
                null, handler => handler.Dispose());
    }

    /// <summary>
    /// UI事件 4个泛型参数
    /// </summary>
    public sealed class UITaskEventHandleP4<P1, P2, P3, P4>
    {
        private LinkedList<UITaskEventHandleP4<P1, P2, P3, P4>>     m_UITaskEventList;
        private LinkedListNode<UITaskEventHandleP4<P1, P2, P3, P4>> m_UITaskEventNode;

        private UITaskEventDelegate<P1, P2, P3, P4> m_UITaskEventParamDelegate;
        public  UITaskEventDelegate<P1, P2, P3, P4> UITaskEventParamDelegate => m_UITaskEventParamDelegate;

        public UITaskEventHandleP4()
        {
        }

        internal UITaskEventHandleP4<P1, P2, P3, P4> Init(
            LinkedList<UITaskEventHandleP4<P1, P2, P3, P4>>     uiTaskEventList,
            LinkedListNode<UITaskEventHandleP4<P1, P2, P3, P4>> uiTaskEventNode,
            UITaskEventDelegate<P1, P2, P3, P4>                 uiTaskEventDelegate)
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