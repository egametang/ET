using System.Collections.Generic;
using Base;

namespace Model
{
	[EntityEvent(typeof(GateSessionKeyComponent))]
	public class GateSessionKeyComponent : Component
	{
		private TimerComponent timerComponent;

		private readonly HashSet<long> sessionKey = new HashSet<long>();

		private void Awake()
		{
			this.timerComponent = Game.Scene.GetComponent<TimerComponent>();
		}

		public long Get()
		{
			long key = RandomHelper.RandInt64();
			this.sessionKey.Add(key);
			this.TimeoutRemoveKey(key);
			return key;
		}

		public bool Check(long key)
		{
			bool ret = this.sessionKey.Contains(key);
			if (ret)
			{
				this.sessionKey.Remove(key);
			}
			return ret;
		}

		public void Remove(long key)
		{
			this.sessionKey.Remove(key);
		}

		private async void TimeoutRemoveKey(long key)
		{
			await this.timerComponent.WaitAsync(20000);
			this.sessionKey.Remove(key);
		}
	}
}
