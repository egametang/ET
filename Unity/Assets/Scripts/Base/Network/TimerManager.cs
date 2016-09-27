using System;
using System.Collections.Generic;

namespace Base
{
	public class TimerManager
	{
		private class Timer
		{
			public long Id { get; set; }
			public long Time { get; set; }
			public Action Action { get; set; }
		}

		private readonly Dictionary<long, Timer> timers = new Dictionary<long, Timer>();

		/// <summary>
		/// key: time, value: timer id
		/// </summary>
		private readonly MultiMap<long, long> timeGuid = new MultiMap<long, long>();

		private readonly Queue<long> timeoutTimer = new Queue<long>();

		public long Add(long time, Action action)
		{
			Timer timer = new Timer { Id = IdGenerater.GenerateId(), Time = time, Action = action };
			this.timers[timer.Id] = timer;
			this.timeGuid.Add(timer.Time, timer.Id);
			return timer.Id;
		}

		public void Remove(long id)
		{
			Timer timer;
			if (!this.timers.TryGetValue(id, out timer))
			{
				return;
			}
			this.timers.Remove(timer.Id);
			this.timeGuid.Remove(timer.Time, timer.Id);
		}

		public void Update()
		{
			long timeNow = TimeHelper.ClientNow();
			foreach (long time in this.timeGuid.Keys)
			{
				if (time > timeNow)
				{
					break;
				}
				this.timeoutTimer.Enqueue(time);
			}

			while (this.timeoutTimer.Count > 0)
			{
				long key = this.timeoutTimer.Dequeue();
				long[] timeoutIds = this.timeGuid.GetAll(key);
				foreach (long id in timeoutIds)
				{
					Timer timer;
					if (!this.timers.TryGetValue(id, out timer))
					{
						continue;
					}
					this.Remove(id);
					timer.Action();
				}
			}
		}
	}
}