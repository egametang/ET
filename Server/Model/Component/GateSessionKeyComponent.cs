﻿using System.Collections.Generic;

namespace Model
{
	public class GateSessionKeyComponent : Component
	{
		private readonly Dictionary<long, string> sessionKey = new Dictionary<long, string>();
		
		public void Add(long key, string account)
		{
			this.sessionKey.Add(key, account);
			this.TimeoutRemoveKey(key);
		}

		public string Get(long key)
		{
			string account = null;
			this.sessionKey.TryGetValue(key, out account);
			return account;
		}

		public void Remove(long key)
		{
			this.sessionKey.Remove(key);
		}

		private async void TimeoutRemoveKey(long key)
		{
			await Game.Scene.GetComponent<TimerComponent>().WaitAsync(20000);
			this.sessionKey.Remove(key);
		}
	}
}
