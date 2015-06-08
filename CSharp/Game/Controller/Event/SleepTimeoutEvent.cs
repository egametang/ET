using System.Threading.Tasks;
using Model;

namespace Controller
{
	[Event(EventType.SleepTimeout, ServerType.City)]
	public class SleepTimeoutEvent_RunTcs : AEvent<Env>
	{
		public override void Run(Env env)
		{
			TaskCompletionSource<bool> tcs =
					env.Get<TaskCompletionSource<bool>>(EnvKey.SleepTimeout_TaskCompletionSource);
			tcs.SetResult(true);
		}
	}
}