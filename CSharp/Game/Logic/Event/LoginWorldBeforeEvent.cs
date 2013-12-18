using Component;
using Log;

namespace Logic
{
	[EventAttribute(Type = EventType.LoginWorldBeforeEvent, Number = EventNumber.CheckPlayerEvent)]
	public class CheckPlayerEvent : IEvent
	{
		public void Trigger(MessageEnv messageEnv)
		{
			Logger.Trace("check player");
		}
	}
}
