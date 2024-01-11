using System.Collections.Generic;

namespace YIUIFramework
{
    /// <summary>
    /// UI事件全局对象池
    /// </summary>
    public static class PublicUITaskEventP3<P1, P2, P3>
    {
        public static readonly ObjectPool<UITaskEventHandleP3<P1, P2, P3>> HandlerPool =
            new ObjectPool<UITaskEventHandleP3<P1, P2, P3>>(
                null, handler => handler.Dispose());
    }

    /// <summary>
    /// UI事件 3个泛型参数
    /// </summary>
    public sealed class UITaskEventHandleP3<P1, P2, P3>
    {
        private LinkedList<UITaskEventHandleP3<P1, P2, P3>>     m_UITaskEventList;
        private LinkedListNode<UITaskEventHandleP3<P1, P2, P3>> m_UITaskEventNode;

        private UITaskEventDelegate<P1, P2, P3> m_UITaskEventParamDelegate;
        public  UITaskEventDelegate<P1, P2, P3> UITaskEventParamDelegate => m_UITaskEventParamDelegate;

        public UITaskEventHandleP3()
        {
        }

        internal UITaskEventHandleP3<P1, P2, P3> Init(
            LinkedList<UITaskEventHandleP3<P1, P2, P3>>     uiTaskEventList,
            LinkedListNode<UITaskEventHandleP3<P1, P2, P3>> uiTaskEventNode,
            UITaskEventDelegate<P1, P2, P3>                 uiTaskEventDelegate)
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