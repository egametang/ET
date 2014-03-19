using System;
using ZeroMQ;

namespace Zmq
{
	public class ZPoller: Poller
	{
		private readonly object eventsLock = new object();
		private Action events = () => {};
		private bool isRunning = true;

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
				local = this.events;
				this.events = () => {};
			}
			local();
		}

		public void Add(ZSocket socket)
		{
			this.AddSocket(socket.ZmqSocket);
		}

		public void Start()
		{
			isRunning = true;
			this.OnEvents();
			while (isRunning)
			{
				this.Poll(TimeSpan.FromMilliseconds(0));
			}
		}

		public void Stop()
		{
			isRunning = false;
		}
	}
}
