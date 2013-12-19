using Component;
using Log;

namespace Logic
{
	[EventAttribute(Type = EventType.BeforeLoginWorldEvent, Number = 1)]
	public class CheckPlayerEvent : IEvent
	{
		public void Trigger(MessageEnv messageEnv)
		{
			Logger.Trace("check player");
		}
	}
}
