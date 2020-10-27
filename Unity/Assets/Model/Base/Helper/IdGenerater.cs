using System.Runtime.InteropServices;

namespace ETModel
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct IdStruct
	{
		public uint Time; // 30bit
		public ushort Value; // 16bit
		public int AppId; // 18bit

		public long ToLong()
		{
			ulong result = 0;
			result |= (uint)this.AppId;
			result |= (ulong)this.Value << 18;
			result |= (ulong)this.Time << 34;
			return (long)result;
		}

		public IdStruct(int appId, uint time, ushort value)
		{
			this.AppId = appId;
			this.Time = time;
			this.Value = value;
		}

		public IdStruct(long id)
		{
			ulong result = (ulong)id;
			this.AppId = (int)(result & 0x03ffff);
			result >>= 18;
			this.Value = (ushort)(result & (ushort.MaxValue));
			result >>= 16;
			this.Time = (uint)result;
		}

		public override string ToString()
		{
			return $"appID: {this.AppId}, time: {this.Time}, value: {this.Value}";
		}
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct InstanceIdStruct
	{
		public ulong Value; // 46bit
		public int AppId; // 18bit

		public long ToLong()
		{
			ulong result = 0;
			result |= (uint)this.AppId;
			result |= this.Value << 18;
			return (long)result;
		}

		public InstanceIdStruct(long id)
		{
			ulong result = (ulong)id;
			this.AppId = (int)(result & 0x03ffff);
			result >>= 18;
			this.Value = result;
		}

		public InstanceIdStruct(int appId, ulong value)
		{
			this.AppId = appId;
			this.Value = value;
		}

		public override string ToString()
		{
			return $"appId: {this.AppId}, value: {this.Value}";
		}
	}
	public static class IdGenerater
	{
		private static int appId;

		public static int AppId
		{
			set
			{
				appId = value;
			}
		}


		private static uint value;
		public static long lastTime;

		public static long GenerateId()
		{
			long time = TimeHelper.ClientNowSeconds();
			if (time != lastTime)
			{
				value = 0;
				lastTime = time;
			}

			if (++value > ushort.MaxValue - 1)
			{
				Log.Error($"id is not enough! value: {value}");
			}

			if (time > int.MaxValue)
			{
				Log.Error($"time > int.MaxValue value: {time}");
			}

			IdStruct idStruct = new IdStruct(appId, (uint)time, (ushort)value);
			return idStruct.ToLong();
		}


		public static ulong instanceIdValue = 0;
		public static long GenerateInstanceId()
		{
			InstanceIdStruct instanceIdStruct = new InstanceIdStruct(appId, ++instanceIdValue);
			return instanceIdStruct.ToLong();
		}

		public static int GetAppId(long v)
		{
			return new IdStruct(v).AppId;
		}
	}
}