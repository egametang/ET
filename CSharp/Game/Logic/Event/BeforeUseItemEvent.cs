using Component;
using Log;

namespace Logic
{
	[EventAttribute(Type = EventType.BeforeUseItemEvent, Order = 1)]
	public class UseCountStatisticsEvent : IEvent
	{
		public void Trigger(MessageEnv messageEnv)
		{
			Logger.Trace("check player");
		}
	}
}
