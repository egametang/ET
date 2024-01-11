using System.Collections.Generic;

namespace YIUIFramework
{
    /// <summary>
    /// UI事件全局对象池
    /// </summary>
    public static class PublicUITaskEventP0
    {
        public static readonly ObjectPool<UITaskEventHandleP0> HandlerPool = new ObjectPool<UITaskEventHandleP0>(
            null, handler => handler.Dispose());
    }

    /// <summary>
    /// UI事件 无参数
    /// </summary>
    public sealed class UITaskEventHandleP0
    {
        private LinkedList<UITaskEventHandleP0>     m_UITaskEventList;
        private LinkedListNode<UITaskEventHandleP0> m_UITaskEventNode;

        private UITaskEventDelegate m_UITaskEventParamDelegate;
        public  UITaskEventDelegate UITaskEventParamDelegate => m_UITaskEventParamDelegate;

        public UITaskEventHandleP0()
        {
        }

        internal UITaskEventHandleP0 Init(
            LinkedList<UITaskEventHandleP0>     uiTaskList,
            LinkedListNode<UITaskEventHandleP0> uiTaskNode,
            UITaskEventDelegate                 uiTaskDelegate)
        {
            m_UITaskEventList          = uiTaskList;
            m_UITaskEventNode          = uiTaskNode;
            m_UITaskEventParamDelegate = uiTaskDelegate;
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