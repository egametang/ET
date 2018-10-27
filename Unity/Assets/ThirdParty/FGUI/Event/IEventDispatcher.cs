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
	}
}
