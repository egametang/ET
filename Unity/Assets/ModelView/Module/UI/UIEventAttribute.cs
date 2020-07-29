using System;

namespace ET
{
    public class UIEventAttribute: Attribute
    {
        public string UIType { get; }

        public UIEventAttribute(string uiType)
        {
            this.UIType = uiType;
        }
    }
}