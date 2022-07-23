using System;
using System.Collections.Generic;

namespace FairyGUI
{
    /// <summary>
    /// 
    /// </summary>
    public interface IEventDispatcher
    {
        void AddEventListener(string strType, EventCallback0 callback);
        void AddEventListener(string strType, EventCallback1 callback);
        void RemoveEventListener(string strType, EventCallback0 callback);
        void RemoveEventListener(string strType, EventCallback1 callback);
        bool DispatchEvent(EventContext context);
        bool DispatchEvent(string strType);
        bool DispatchEvent(string strType, object data);
        bool DispatchEvent(string strType, object data, object initiator);
    }
}
