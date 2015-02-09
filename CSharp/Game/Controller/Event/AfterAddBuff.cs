using Common.Event;
using Model;

namespace Controller
{
	[Event(EventType.AfterAddBuff)]
	public class AddBuffToTimer: IEventSync
	{
		public void Run(Env env)
		{
		}
	}
}