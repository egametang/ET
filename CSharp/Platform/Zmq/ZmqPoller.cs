using System;
using NetMQ;

namespace Zmq
{
	public class ZmqPoller: Poller
	{
		private readonly object eventsLock = new object();
		private Action events;

		public ZmqPoller()
		{
			AddTimer();
		}

		private void AddTimer()
		{
			var timer = new NetMQTimer(TimeSpan.FromMilliseconds(10));
			timer.Elapsed += (sender, args) => this.OnEvents();
			AddTimer(timer);
		}

		public event Action Events
		{
			add
			{
				lock (this.eventsLock)
				{
					this.events += value;
				}
			}
			remove
			{
				lock (this.eventsLock)
				{
					this.events -= value;
				}
			}
		}

		private void OnEvents()
		{
			Action local = null;
			lock (this.eventsLock)
			{
				if (this.events == null)
				{
					return;
				}
				local = this.events;
				this.events = null;
			}
			local();
			AddTimer();
		}
	}
}
