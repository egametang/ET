using System.Threading;

namespace Model
{
	public class OneThreadSynchronizationContext : SynchronizationContext
	{
		public override void Post(SendOrPostCallback callback, object state)
		{
			Game.Poller.Add(() => { callback(state); });
		}
	}
}
