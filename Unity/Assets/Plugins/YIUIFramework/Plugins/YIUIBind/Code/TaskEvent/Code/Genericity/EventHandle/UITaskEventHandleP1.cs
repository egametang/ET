using System.Collections.Generic;

namespace YIUIFramework
{
    /// <summary>
    /// UI事件全局对象池
    /// </summary>
    public static class PublicUITaskEventP1<P1>
    {
        public static readonly ObjectPool<UITaskEventHandleP1<P1>> HandlerPool = new ObjectPool<UITaskEventHandleP1<P1>>(
            null, handler => handler.Dispose());
    }

    /// <summary>
    /// UI事件 1个泛型参数
    /// </summary>
    public sealed class UITaskEventHandleP1<P1>
    {
        private LinkedList<UITaskEventHandleP1<P1>>     m_UITaskEventList;
        private LinkedListNode<UITaskEventHandleP1<P1>> m_UITaskEventNode;

        private UITaskEventDelegate<P1> m_UITaskEventParamDelegate;
        public  UITaskEventDelegate<P1> UITaskEventParamDelegate => m_UITaskEventParamDelegate;

        public UITaskEventHandleP1()
        {
        }

        internal UITaskEventHandleP1<P1> Init(
            LinkedList<UITaskEventHandleP1<P1>>     uiTaskEventList,
            LinkedListNode<UITaskEventHandleP1<P1>> uiTaskEventNode,
            UITaskEventDelegate<P1>                 uiTaskEventDelegate)
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