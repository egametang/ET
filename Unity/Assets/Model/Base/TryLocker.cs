using System;
using System.Threading;

namespace ET
{
	public class TryLock : IDisposable
	{
		private object locked;

		public bool HasLock { get; private set; }

		public TryLock(object obj)
		{
			if (!Monitor.TryEnter(obj))
			{
				return;
			}

			this.HasLock = true;
			this.locked = obj;
		}

		public void Dispose()
		{
			if (!this.HasLock)
			{
				return;
			}

			Monitor.Exit(this.locked);
			this.locked = null;
			this.HasLock = false;
		}
	}
}
