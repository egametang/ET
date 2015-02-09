using Common.Event;

namespace Model
{
	public class EventAttribute: AEventAttribute
	{
		public EventAttribute(int type): base(type)
		{
		}
	}

	public class ActionAttribute: AEventAttribute
	{
		public ActionAttribute(int type): base(type)
		{
		}
	}
}