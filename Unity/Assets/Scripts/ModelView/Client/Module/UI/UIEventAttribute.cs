using System;

namespace ET.Client
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