using System.Collections.Generic;

namespace FairyGUI
{
    /// <summary>
    /// 
    /// </summary>
    public class EventContext
    {
        /// <summary>
        /// 
        /// </summary>
        public EventDispatcher sender { get; internal set; }

        /// <summary>
        /// /
        /// </summary>
        public object initiator { get; internal set; }

        /// <summary>
        /// /
        /// </summary>
        public InputEvent inputEvent { get; internal set; }

        /// <summary>
        /// 
        /// </summary>
        public string type;

        /// <summary>
        /// 
        /// </summary>
        public object data;

        internal bool _defaultPrevented;
        internal bool _stopsPropagation;
        internal bool _touchCapture;

        internal List<EventBridge> callChain = new List<EventBridge>();

        /// <summary>
        /// 
        /// </summary>
        public void StopPropagation()
        {
            _stopsPropagation = true;
        }

        /// <summary>
        /// 
        /// </summary>
        public void PreventDefault()
        {
            _defaultPrevented = true;
        }

        /// <summary>
        /// 
        /// </summary>
        public void CaptureTouch()
        {
            _touchCapture = true;
        }

        /// <summary>
        /// 
        /// </summary>
        public bool isDefaultPrevented
        {
            get { return _defaultPrevented; }
        }

        static Stack<EventContext> pool = new Stack<EventContext>();
        internal static EventContext Get()
        {
            if (pool.Count > 0)
            {
                EventContext context = pool.Pop();
                context._stopsPropagation = false;
                context._defaultPrevented = false;
                context._touchCapture = false;
                return context;
            }
            else
                return new EventContext();
        }

        internal static void Return(EventContext value)
        {
            pool.Push(value);
        }
    }

}
