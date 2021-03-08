using System;
using System.Runtime.InteropServices;

namespace ET
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct IdStruct
    {
        public uint Time;    // 30bit
        public int Process;  // 18bit
        public ushort Value; // 16bit

        public long ToLong()
        {
            ulong result = 0;
            result |= this.Value;
            result |= (ulong) this.Process << 16;
            result |= (ulong) this.Time << 34;
            return (long) result;
        }

        public IdStruct(uint time, int process, ushort value)
        {
            this.Process = process;
            this.Time = time;
            this.Value = value;
        }

        public IdStruct(long id)
        {
            ulong result = (ulong) id;
            this.Value = (ushort) (result & ushort.MaxValue);
            result >>= 16;
            this.Process = (int) (result & IdGenerater.Mask18bit);
            result >>= 18;
            this.Time = (uint) result;
        }

        public override string ToString()
        {
            return $"process: {this.Process}, time: {this.Time}, value: {this.Value}";
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct InstanceIdStruct
    {
        public uint Time;   // 当年开始的tick 28bit
        public int Process; // 18bit
        public uint Value;  // 18bit

        public long ToLong()
        {
            ulong result = 0;
            result |= this.Value;
            result |= (ulong)this.Process << 18;
            result |= (ulong) this.Time << 36;
            return (long) result;
        }

        public InstanceIdStruct(long id)
        {
            ulong result = (ulong) id;
            this.Value = (uint)(result & IdGenerater.Mask18bit);
            result >>= 18;
            this.Process = (int)(result & IdGenerater.Mask18bit);
            result >>= 18;
            this.Time = (uint)result;
        }

        public InstanceIdStruct(uint time, int process, uint value)
        {
            this.Time = time;
            this.Process = process;
            this.Value = value;
        }
        
        // 给SceneId使用
        public InstanceIdStruct(int process, uint value)
        {
            this.Time = 0;
            this.Process = process;
            this.Value = value;
        }

        public override string ToString()
        {
            return $"process: {this.Process}, value: {this.Value} time: {this.Time}";
        }
    }
    
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct UnitIdStruct
    {
        public uint Time;        // 30bit 34年
        public ushort Zone;      // 10bit 1024个区
        public byte ProcessMode; // 8bit  Process % 256  一个区最多256个进程
        public ushort Value;     // 16bit 每秒每个进程最大16K个Unit

        public long ToLong()
        {
            ulong result = 0;

            result |= 1ul << 63; // 最高位变成1，暂时让它跟普通id区分一下，正式版删除
            
            result |= this.Value;
            result |= (uint)this.ProcessMode << 16;
            result |= (ulong) this.Zone << 24;
            result |= (ulong) this.Time << 34;
            return (long) result;
        }

        public UnitIdStruct(int zone, int process, uint time, ushort value)
        {
            this.Time = time;
            this.ProcessMode = (byte)(process % 256);
            this.Value = value;
            this.Zone = (ushort)zone;
        }
                        
        public override string ToString()
        {
            return $"ProcessMode: {this.ProcessMode}, value: {this.Value} time: {this.Time}";
        }
        
        public static int GetUnitZone(long unitId)
        {
            int v = (int) ((unitId >> 24) & 0x03ff); // 取出10bit
            return v;
        }
    }

    public class IdGenerater: IDisposable
    {
        public const int Mask18bit = 0x03ffff;
        public static IdGenerater Instance = new IdGenerater();

        public const int MaxZone = 1024;
        
        private long epoch2020;
        private ushort value;
        private uint lastIdTime;
        private ushort idThisSecCount;

        
        private long instanceIdEpoch;
        private uint instanceIdValue;
        private uint lastInstanceIdTime;
        private uint instanceIdThisSecCount;
        
        
        private ushort unitIdValue;
        private uint lastUnitIdTime;
        private ushort unitIdThisSecCount;

        public IdGenerater()
        {
            long epoch1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).Ticks / 10000;
            this.epoch2020 = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc).Ticks / 10000 - epoch1970;
            this.instanceIdEpoch = new DateTime(DateTime.Now.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc).Ticks / 10000 - epoch1970;
        }

        public void Dispose()
        {
            this.epoch2020 = 0;
            this.instanceIdEpoch = 0;
            this.value = 0;
        }

        private uint TimeSince2020()
        {
            return (uint)((Game.TimeInfo.FrameTime - this.epoch2020) / 1000);
        }
        
        private uint TimeSinceThisYear()
        {
            return (uint)((Game.TimeInfo.FrameTime - this.instanceIdEpoch) / 1000);
        }

        public long GenerateInstanceId()
        {
            uint time = TimeSinceThisYear();
            
            if (time == this.lastInstanceIdTime)
            {
                ++this.instanceIdThisSecCount;
            }
            else
            {
                this.lastInstanceIdTime = time;
                this.instanceIdThisSecCount = 1;
            }
            if (this.instanceIdThisSecCount > IdGenerater.Mask18bit - 1)
            {
                Log.Error($"instanceid count per sec overflow: {this.instanceIdThisSecCount}");
            }
            
            
            if (++this.instanceIdValue > IdGenerater.Mask18bit - 1) // 18bit
            {
                this.instanceIdValue = 0;
            }
            InstanceIdStruct instanceIdStruct = new InstanceIdStruct(time, Game.Options.Process, this.instanceIdValue);
            return instanceIdStruct.ToLong();
        }

        public long GenerateId()
        {
            uint time = TimeSince2020();
            
            if (time == lastIdTime)
            {
                ++this.idThisSecCount;
            }
            else
            {
                this.lastIdTime = time;
                this.idThisSecCount = 1;
            }
            if (this.idThisSecCount == ushort.MaxValue)
            {
                Log.Error($"id count per sec overflow: {this.idThisSecCount}");
            }


            if (++value > ushort.MaxValue - 1)
            {
                this.value = 0;
            }
            IdStruct idStruct = new IdStruct(time, Game.Options.Process, value);
            return idStruct.ToLong();
        }
        
        public long GenerateUnitId(int zone)
        {
            if (zone > MaxZone)
            {
                throw new Exception($"zone > MaxZone: {zone}");
            }
            uint time = TimeSince2020();
            
            
            if (time == this.lastUnitIdTime)
            {
                ++this.unitIdThisSecCount;
            }
            else
            {
                this.lastUnitIdTime = time;
                this.unitIdThisSecCount = 1;
            }
            if (this.unitIdThisSecCount == ushort.MaxValue)
            {
                Log.Error($"unitid count per sec overflow: {this.unitIdThisSecCount}");
            }

            if (++this.unitIdValue > ushort.MaxValue - 1)
            {
                this.unitIdValue = 0;
            }

            UnitIdStruct unitIdStruct = new UnitIdStruct(zone, Game.Options.Process, time, this.unitIdValue);
            return unitIdStruct.ToLong();
        }
    }
}