using System.Collections.Generic;
using YIUIFramework;

namespace YIUIFramework
{
    /// <summary>
    /// UI事件全局对象池
    /// </summary>
    public static class PublicUIEventP1<P1>
    {
        public static readonly ObjectPool<UIEventHandleP1<P1>> HandlerPool = new ObjectPool<UIEventHandleP1<P1>>(
            null, handler => handler.Dispose());
    }

    /// <summary>
    /// UI事件 1个泛型参数
    /// </summary>
    public sealed class UIEventHandleP1<P1>
    {
        private LinkedList<UIEventHandleP1<P1>>     m_UIEventList;
        private LinkedListNode<UIEventHandleP1<P1>> m_UIEventNode;

        private UIEventDelegate<P1> m_UIEventParamDelegate;
        public  UIEventDelegate<P1> UIEventParamDelegate => m_UIEventParamDelegate;

        public UIEventHandleP1()
        {
        }

        internal UIEventHandleP1<P1> Init(
            LinkedList<UIEventHandleP1<P1>>     uiEventList,
            LinkedListNode<UIEventHandleP1<P1>> uiEventNode,
            UIEventDelegate<P1>                 uiEventDelegate)
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