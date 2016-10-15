using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
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
		private readonly MultiMap<long, ObjectId> timeId = new MultiMap<long, ObjectId>();

		private readonly Queue<long> timeoutTimer = new Queue<long>();

		public ObjectId Add(long time, Action action)
		{
			Timer timer = new Timer { Id = ObjectId.GenerateNewId(), Time = time, Action = action };
			this.timers[timer.Id] = timer;
			this.timeId.Add(timer.Time, timer.Id);
			return timer.Id;
		}

		public void Remove(ObjectId id)
		{
			Timer timer;
			if (!this.timers.TryGetValue(id, out timer))
			{
				return;
			}
			this.timers.Remove(timer.Id);
			this.timeId.Remove(timer.Time, timer.Id);
		}

		public Task<bool> Sleep(int time)
		{
			TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
			this.Add(TimeHelper.Now() + time, () => { tcs.SetResult(true); });
			return tcs.Task;
		}

		public Task<bool> Sleep(int time, CancellationToken token)
		{
			TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
			ObjectId id = this.Add(TimeHelper.Now() + time, () => { tcs.SetResult(true); });
			token.Register(() => { this.Remove(id); });
			return tcs.Task;
		}

		public void Refresh()
		{
			long timeNow = TimeHelper.Now();

			while (true)
			{
				KeyValuePair<long, List<ObjectId>> first = this.timeId.First;
				if (first.Key > timeNow)
				{
					return;
				}

				List<ObjectId> timeoutId = first.Value;
				this.timeId.Remove(first.Key);
				foreach (ObjectId id in timeoutId)
				{
					Timer timer;
					if (!this.timers.TryGetValue(id, out timer))
					{
						continue;
					}
					this.timers.Remove(id);
					timer.Action();
				}
			}
		}
	}
}