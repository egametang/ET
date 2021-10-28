using System;

namespace ET
{
    public class SessionStreamDispatcherAttribute: BaseAttribute
    {
        public int Type;

        public SessionStreamDispatcherAttribute(int type)
        {
            this.Type = type;
        }
    }
}