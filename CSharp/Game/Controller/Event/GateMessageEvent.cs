using Model;

namespace Controller
{
	[Event(EventType.GateMessage, ServerType.Gate)]
	public class GateMessageEvent : IEventSync
	{
		public void Run(Env env)
		{
		}
	}
}