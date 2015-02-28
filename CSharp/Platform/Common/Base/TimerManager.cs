using System;
using System.Collections.Generic;
using Common.Helper;
using MongoDB.Bson;

namespace Common.Base
{
	public class TimerManager
	{
		private class Timer
		{
			public ObjectId Id { get; set; }
			public long Time { get; set; }
			public Action Action { get; set; }
		}

		private readonly Dictionary<ObjectId, Timer> timers = new Dictionary<ObjectId, Timer>();

		/// <summary>
		/// key: time, value: timer id
		/// </summary>
		private readonly MultiMap<long, ObjectId> timeGuid = new MultiMap<long, ObjectId>();

		private readonly Queue<long> timeoutTimer = new Queue<long>();

		public ObjectId Add(long time, Action action)
		{
			Timer timer = new Timer { Id = ObjectId.GenerateNewId(), Time = time, Action = action };
			this.timers[timer.Id] = timer;
			this.timeGuid.Add(timer.Time, timer.Id);
			return timer.Id;
		}

		public void Update(ObjectId id, long time)
		{
			Timer timer;
			if (!this.timers.TryGetValue(id, out timer))
			{
				return;
			}

			this.timeGuid.Remove(timer.Time, timer.Id);
			timer.Time = time;
			this.timeGuid.Add(timer.Time, timer.Id);
		}

		public void Remove(ObjectId id)
		{
			Timer timer;
			if (!this.timers.TryGetValue(id, out timer))
			{
				return;
			}
			this.timers.Remove(timer.Id);
			this.timeGuid.Remove(timer.Time, timer.Id);
		}

		public void Refresh()
		{
			long timeNow = TimeHelper.Now();
			foreach (long time in this.timeGuid.Keys)
			{
				if (time > timeNow)
				{
					break;
				}
				timeoutTimer.Enqueue(time);
			}

			while (timeoutTimer.Count > 0)
			{
				long key = timeoutTimer.Dequeue();
				ObjectId[] timeoutIds = this.timeGuid.GetAll(key);
				foreach (ObjectId id in timeoutIds)
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