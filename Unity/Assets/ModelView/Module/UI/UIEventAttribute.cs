using System;

namespace ET
{
    public class UIEventAttribute: BaseAttribute
    {
        public string UIType { get; }

        public UIEventAttribute(string uiType)
        {
            this.UIType = uiType;
        }
    }
}