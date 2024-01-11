using System.Collections.Generic;

namespace YIUIFramework
{
    /// <summary>
    /// UI事件全局对象池
    /// </summary>
    public static class PublicUITaskEventP2<P1, P2>
    {
        public static readonly ObjectPool<UITaskEventHandleP2<P1, P2>> HandlerPool =
            new ObjectPool<UITaskEventHandleP2<P1, P2>>(
                null, handler => handler.Dispose());
    }

    /// <summary>
    /// UI事件 2个泛型参数
    /// </summary>
    public sealed class UITaskEventHandleP2<P1, P2>
    {
        private LinkedList<UITaskEventHandleP2<P1, P2>>     m_UITaskEventList;
        private LinkedListNode<UITaskEventHandleP2<P1, P2>> m_UITaskEventNode;

        private UITaskEventDelegate<P1, P2> m_UITaskEventParamDelegate;
        public  UITaskEventDelegate<P1, P2> UITaskEventParamDelegate => m_UITaskEventParamDelegate;

        public UITaskEventHandleP2()
        {
        }

        internal UITaskEventHandleP2<P1, P2> Init(
            LinkedList<UITaskEventHandleP2<P1, P2>>     uiTaskEventList,
            LinkedListNode<UITaskEventHandleP2<P1, P2>> uiTaskEventNode,
            UITaskEventDelegate<P1, P2>                 uiTaskEventDelegate)
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