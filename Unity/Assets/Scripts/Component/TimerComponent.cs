using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ETModel
{
	public struct Timer
	{
		public long Id { get; set; }
		public long Time { get; set; }
		public TaskCompletionSource<bool> tcs;
	}

	[ObjectSystem]
	public class TimerComponentUpdateSystem : UpdateSystem<TimerComponent>
	{
		public override void Update(TimerComponent self)
		{
			self.Update();
		}
	}

	public class TimerComponent : Component
	{
		private readonly Dictionary<long, Timer> timers = new Dictionary<long, Timer>();

		/// <summary>
		/// key: time, value: timer id
		/// </summary>
		private readonly MultiMap<long, long> timeId = new MultiMap<long, long>();

		private readonly List<long> timeOutId = new List<long>();

		public void Update()
		{
			if (this.timeId.Count == 0)
			{
				return;
			}

			long timeNow = TimeHelper.Now();

			timeOutId.Clear();

			while (this.timeId.Count > 0)
			{
				long k = this.timeId.FirstKey();
				if (k > timeNow)
				{
					break;
				}
				foreach (long ll in this.timeId[k])
				{
					this.timeOutId.Add(ll);
				}
				this.timeId.Remove(k);
			}

			foreach (long k in this.timeOutId)
			{
				Timer timer;
				if (!this.timers.TryGetValue(k, out timer))
				{
					continue;
				}
				this.timers.Remove(k);
				timer.tcs.SetResult(true);
			}
		}

		private void Remove(long id)
		{
			Timer timer;
			if (!this.timers.TryGetValue(id, out timer))
			{
				return;
			}
			this.timers.Remove(id);
		}

		public Task WaitTillAsync(long tillTime, CancellationToken cancellationToken)
		{
			TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
			Timer timer = new Timer { Id = IdGenerater.GenerateId(), Time = tillTime, tcs = tcs };
			this.timers[timer.Id] = timer;
			this.timeId.Add(timer.Time, timer.Id);
			cancellationToken.Register(() => { this.Remove(timer.Id); });
			return tcs.Task;
		}

		public Task WaitTillAsync(long tillTime)
		{
			TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
			Timer timer = new Timer { Id = IdGenerater.GenerateId(), Time = tillTime, tcs = tcs };
			this.timers[timer.Id] = timer;
			this.timeId.Add(timer.Time, timer.Id);
			return tcs.Task;
		}

		public Task WaitAsync(long time, CancellationToken cancellationToken)
		{
			TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
			Timer timer = new Timer { Id = IdGenerater.GenerateId(), Time = TimeHelper.Now() + time, tcs = tcs };
			this.timers[timer.Id] = timer;
			this.timeId.Add(timer.Time, timer.Id);
			cancellationToken.Register(() => { this.Remove(timer.Id); });
			return tcs.Task;
		}

		public Task WaitAsync(long time)
		{
			TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
			Timer timer = new Timer { Id = IdGenerater.GenerateId(), Time = TimeHelper.Now() + time, tcs = tcs };
			this.timers[timer.Id] = timer;
			this.timeId.Add(timer.Time, timer.Id);
			return tcs.Task;
		}
	}
}