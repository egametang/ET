using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Common.Base;
using Common.Helper;
using MongoDB.Bson;

namespace Model
{
	public class TimerComponent: Component<World>, IUpdate
	{
		private class Timer
		{
			public ObjectId Id { get; set; }
			public long Time { get; set; }
			public EventType CallbackEvent { get; set; }
			public Env Env { get; set; }
		}

		private readonly Dictionary<ObjectId, Timer> timers = new Dictionary<ObjectId, Timer>();

		/// <summary>
		/// key: time, value: timer id
		/// </summary>
		private readonly MultiMap<long, ObjectId> timeId = new MultiMap<long, ObjectId>();
		
		public ObjectId Add(long time, EventType callbackEvent, Env env)
		{
			Timer timer = new Timer
			{
				Id = ObjectId.GenerateNewId(),
				Time = time,
				CallbackEvent = callbackEvent,
				Env = env
			};
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
			this.timeId.Remove(timer.Time, timer.Id);
		}

		public Task<bool> Sleep(int time)
		{
			TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
			Env env = new Env();
			env[EnvKey.SleepTimeout_TaskCompletionSource] = tcs;
			this.Add(TimeHelper.Now() + time, EventType.SleepTimeout, env);
			return tcs.Task;
		}

		public void Update()
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
#pragma warning disable 4014
					World.Instance.GetComponent<EventComponent<EventAttribute>>().RunAsync(timer.CallbackEvent, timer.Env);
#pragma warning restore 4014
				}
			}
		}
	}
}