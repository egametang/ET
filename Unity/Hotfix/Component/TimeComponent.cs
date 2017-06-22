using Model;

namespace Hotfix
{
	/// <summary>
	/// 用于同步服务端和客户端时间
	/// </summary>
	public class TimeComponent: Component
	{
		private long syncTime;

		private long syncClientTime;

		public long SyncTime
		{
			get
			{
				return this.syncTime;
			}
			set
			{
				this.syncTime = value;
				this.syncClientTime = TimeHelper.ClientNow();
			}
		}

		public long Now()
		{
			return TimeHelper.ClientNow() - this.syncClientTime + syncTime;
		}
	}
}