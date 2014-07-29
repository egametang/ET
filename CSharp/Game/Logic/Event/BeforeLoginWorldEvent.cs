using Component;
using Logger;

namespace Logic
{
    [Event(Type = EventType.BeforeLoginWorldEvent, Order = 1)]
    public class CheckPlayerEvent: IEvent
    {
        public void Trigger(MessageEnv messageEnv)
        {
            Log.Trace("check player");
        }
    }
}