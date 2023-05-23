using System;

namespace ET
{
    [AttributeUsage(AttributeTargets.Class)]
    public class AUIEventAttribute: BaseAttribute
    {
        public WindowID WindowID
        {
            get;
        }

        public AUIEventAttribute(WindowID windowID)
        {
            this.WindowID = windowID;
        }
    }
}