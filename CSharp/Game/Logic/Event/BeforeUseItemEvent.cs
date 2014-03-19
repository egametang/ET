using Component;
using Logger;

namespace Logic
{
	[EventAttribute(Type = EventType.BeforeUseItemEvent, Order = 1)]
	public class UseCountStatisticsEvent : IEvent
	{
		public void Trigger(MessageEnv messageEnv)
		{
			Log.Trace("check player");
		}
	}
}
