using Common.Event;

namespace Model
{
    public class EventAttribute: AEventAttribute
    {
        public EventAttribute(int type): base(type)
        {
        }
    }

    public class CallbackAttribute : AEventAttribute
    {
        public CallbackAttribute(int type): base(type)
        {
        }
    }
}
