using System.Runtime.InteropServices;

 namespace ET
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct IdStruct
	{
		public uint Time; // 30bit
		public ushort Value; // 16bit
		public int Process; // 18bit
		
		public long ToLong()
		{
			ulong result = 0;
			result |= (uint)this.Process;
			result |= (ulong)this.Value << 18;
			result |= (ulong)this.Time << 34;
			return (long)result;
		}
		
		public IdStruct(int process, uint time, ushort value)
		{
			this.Process = process;
			this.Time = time;
			this.Value = value;
		}
		
		public IdStruct(long id)
		{
			ulong result = (ulong) id;
			this.Process = (int)(result & 0x03ffff);
			result >>= 18;
			this.Value = (ushort)(result & (ushort.MaxValue));
			result >>= 16;
			this.Time = (uint)result;
		}

		public override string ToString()
		{
			return $"process: {this.Process}, time: {this.Time}, value: {this.Value}";
		}
	}
	
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct InstanceIdStruct
	{
		public ulong Value; // 46bit
		public int Process; // 18bit
		
		public long ToLong()
		{
			ulong result = 0;
			result |= (uint)this.Process;
			result |= this.Value << 18;
			return (long)result;
		}
		
		public InstanceIdStruct(long id)
		{
			ulong result = (ulong) id;
			this.Process = (int)(result & 0x03ffff);
			result >>= 18;
			this.Value = result;
		}
		
		public InstanceIdStruct(int process, ulong value)
		{
			this.Process = process;
			this.Value = value;
		}

		public override string ToString()
		{
			return $"process: {this.Process}, value: {this.Value}";
		}
	}
	
	public static class IdGenerater
	{
		public const int MaxZone = 1024;
		
		private static int process;
		private static uint value;
		
		public static int Process
		{
			set
			{
				process = value;
			}
			get
			{
				return process;
			}
		}
		
		public static int GetProcess(long v)
		{
			return new IdStruct(v).Process;
		}
		
		// 一个区顶多1000个配置scene
		private static ulong MaxConfigSceneId = 1024 * 1000;
		
		// Scene的InstanceId跟Id一样
		public static long GenerateProcessSceneId()
		{
			InstanceIdStruct instanceIdStruct = new InstanceIdStruct(process, 0);
			return instanceIdStruct.ToLong();
		}

		public static long lastTime;
		
		public static long GenerateInstanceId()
		{
			InstanceIdStruct instanceIdStruct = new InstanceIdStruct(process, ++MaxConfigSceneId);
			return instanceIdStruct.ToLong();
		}
		
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

			IdStruct idStruct = new IdStruct(process, (uint)time, (ushort)value);
			return idStruct.ToLong();
		}
	}
}