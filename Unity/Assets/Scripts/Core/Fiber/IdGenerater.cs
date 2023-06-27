using System;
using System.Runtime.InteropServices;

namespace ET
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct IdStruct
    {
        public short Process;  // 14bit
        public uint Time;    // 30bit
        public uint Value;   // 20bit

        public long ToLong()
        {
            ulong result = 0;
            result |= (ushort) this.Process;
            result <<= 14;
            result |= this.Time;
            result <<= 30;
            result |= this.Value;
            return (long) result;
        }

        public IdStruct(uint time, short process, uint value)
        {
            this.Process = process;
            this.Time = time;
            this.Value = value;
        }

        public IdStruct(long id)
        {
            ulong result = (ulong) id; 
            this.Value = (ushort) (result & IdGenerater.Mask20bit);
            result >>= 20;
            this.Time = (uint) result & IdGenerater.Mask30bit;
            result >>= 30;
            this.Process = (short) (result & IdGenerater.Mask14bit);
        }

        public override string ToString()
        {
            return $"process: {this.Process}, time: {this.Time}, value: {this.Value}";
        }
    }
    
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct InstanceIdStruct
    {
        public uint Time;  // 32bit
        public uint Value; // 32bit

        public long ToLong()
        {
            ulong result = 0;
            result |= this.Time;
            result <<= 32;
            result |= this.Value;
            return (long) result;
        }

        public InstanceIdStruct(uint time, uint value)
        {
            this.Time = time;
            this.Value = value;
        }

        public InstanceIdStruct(long id)
        {
            ulong result = (ulong) id; 
            this.Value = (uint)(result & uint.MaxValue);
            result >>= 32;
            this.Time = (uint)(result & uint.MaxValue);
        }

        public override string ToString()
        {
            return $"time: {this.Time}, value: {this.Value}";
        }
    }

    public class IdGenerater
    {
        public const int MaxZone = 1024;
        
        public const int Mask14bit = 0x3fff;
        public const int Mask30bit = 0x3fffffff;
        public const int Mask20bit = 0xfffff;
        
        private readonly long epoch2022;
        private uint value;
        private uint lastIdTime;

        private readonly TimeInfo timeInfo;
        private readonly int process;
        
        private uint instanceIdValue;
        
        public IdGenerater(int process, TimeInfo timeInfo)
        {
            this.process = process;
            this.timeInfo = timeInfo;
            
            long epoch1970tick = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).Ticks / 10000;
            this.epoch2022 = new DateTime(2022, 1, 1, 0, 0, 0, DateTimeKind.Utc).Ticks / 10000 - epoch1970tick;
            this.lastIdTime = TimeSince2022();
            if (this.lastIdTime <= 0)
            {
                Log.Warning($"lastIdTime less than 0: {this.lastIdTime}");
                this.lastIdTime = 1;
            }
        }

        private uint TimeSince2022()
        {
            uint a = (uint)((this.timeInfo.FrameTime - this.epoch2022) / 1000);
            return a;
        }
        
        public long GenerateId()
        {
            uint time = TimeSince2022();

            // 时间不会倒退
            if (time > this.lastIdTime)
            {
                this.lastIdTime = time;
            }
            this.value = IdValueGenerater.Instance.Value;

            IdStruct idStruct = new(this.lastIdTime, (short)this.process, value);
            return idStruct.ToLong();
        }
        
        public long GenerateInstanceId()
        {
            uint time = this.TimeSince2022();

            // 时间不会倒退
            if (time > this.lastIdTime)
            {
                this.lastIdTime = time;
            }
            ++this.instanceIdValue;
                
            if (this.instanceIdValue >= int.MaxValue)
            {
                this.instanceIdValue = 0;
            }

            InstanceIdStruct instanceIdStruct = new(this.lastIdTime, this.instanceIdValue);
            return instanceIdStruct.ToLong();
        }
    }
}