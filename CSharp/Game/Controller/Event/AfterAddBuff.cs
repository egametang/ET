using Common.Event;
using Model;

namespace Controller
{
	[Event(EventType.AfterAddBuff)]
	public class AddBuffToTimer: IEvent
	{
		public void Run(Env env)
		{
		}
	}
}