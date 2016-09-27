using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;

namespace Base
{
	public class Timer
	{
		public ObjectId Id { get; set; }
		public long Time { get; set; }
		public TaskCompletionSource<bool> tcs;
	}

	[ObjectEvent]
	public class TimerComponentEvent : ObjectEvent<TimerComponent>, IUpdate, IAwake<TimeComponent>
	{
		public void Update()
		{
			TimerComponent component = this.GetValue();
			component.Update();
		}

		public void Awake(TimeComponent p1)
		{
			this.GetValue().TimeComponent = p1;
		}
	}

	public static class TimerComponentExtension
	{
		public static void Update(this TimerComponent component)
		{
			long timeNow = component.TimeComponent.Now();
			foreach (long time in component.timeId.Keys)
			{
				if (time > timeNow)
				{
					break;
				}
				component.timeoutTimer.Enqueue(time);
			}

			while (component.timeoutTimer.Count > 0)
			{
				long key = component.timeoutTimer.Dequeue();
				ObjectId[] timeOutId = component.timeId.GetAll(key);
				foreach (ObjectId id in timeOutId)
				{
					Timer timer;
					if (!component.timers.TryGetValue(id, out timer))
					{
						continue;
					}
					component.Remove(id);
					timer.tcs.SetResult(true);
				}
			}
		}
	}

	public class TimerComponent: Component<Scene>
	{
		public readonly Dictionary<ObjectId, Timer> timers = new Dictionary<ObjectId, Timer>();

		/// <summary>
		/// key: time, value: timer id
		/// </summary>
		public readonly MultiMap<long, ObjectId> timeId = new MultiMap<long, ObjectId>();

		public readonly Queue<long> timeoutTimer = new Queue<long>();

		public TimeComponent TimeComponent { get; set; }

		public void Remove(ObjectId id)
		{
			Timer timer;
			if (!this.timers.TryGetValue(id, out timer))
			{
				return;
			}
			this.timers.Remove(id);
			this.timeId.Remove(timer.Time, timer.Id);
		}

		public Task WaitTillAsync(long tillTime, CancellationToken cancellationToken)
		{
			TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
			Timer timer = new Timer { Id = ObjectId.GenerateNewId(), Time = tillTime, tcs = tcs };
			this.timers[timer.Id] = timer;
			this.timeId.Add(timer.Time, timer.Id);
			cancellationToken.Register(() =>
			{
				this.Remove(timer.Id);
			});
			return tcs.Task;
		}

		public Task WaitTillAsync(long tillTime)
		{
			TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
			Timer timer = new Timer { Id = ObjectId.GenerateNewId(), Time = tillTime, tcs = tcs };
			this.timers[timer.Id] = timer;
			this.timeId.Add(timer.Time, timer.Id);
			return tcs.Task;
		}

		public Task WaitAsync(long time, CancellationToken cancellationToken)
		{
			TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
			Timer timer = new Timer
			{
				Id = ObjectId.GenerateNewId(),
				Time = TimeHelper.Now() + time,
				tcs = tcs
			};
			this.timers[timer.Id] = timer;
			this.timeId.Add(timer.Time, timer.Id);
			cancellationToken.Register(() =>
			{
				this.Remove(timer.Id);
			});
			return tcs.Task;
		}

		public Task WaitAsync(long time)
		{
			TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
			Timer timer = new Timer
			{
				Id = ObjectId.GenerateNewId(),
				Time = TimeHelper.Now() + time,
				tcs = tcs
			};
			this.timers[timer.Id] = timer;
			this.timeId.Add(timer.Time, timer.Id);
			return tcs.Task;
		}
	}
}