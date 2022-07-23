using System;

namespace ET
{

    [AttributeUsage(AttributeTargets.Field)]
    public class FGUIFieldAttribute : BaseAttribute
    {
        public string path = null;
        public string name = null;
        public FGUIFieldAttribute(string path = null, string name = null)
        {
            this.name = name;
            this.path = path;
        }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class FGUIWrappedViewAttribute : BaseAttribute
    {
        public string path = null;
        public string name = null;
        public FGUIWrappedViewAttribute(string path = null, string name = null)
        {
            this.name = name;
            this.path = path;
        }
    }
}