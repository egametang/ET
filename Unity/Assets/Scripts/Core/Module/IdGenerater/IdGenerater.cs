using System;
using System.Runtime.InteropServices;

namespace ET
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct IdStruct
    {
        public uint Time;    // 30bit，表示从2020年开始的秒数，最多可用34年
        public int Process;  // 18bit，表示进程号，最多可有262144个进程
        public ushort Value; // 16bit，表示每秒每个进程生成的ID数量，最多可有65536个

        // 将ID转换为长整型
        public long ToLong()
        {
            ulong result = 0;
            result |= this.Value;
            result |= (ulong) this.Process << 16;
            result |= (ulong) this.Time << 34;
            return (long) result;
        }

        // 根据时间、进程和值创建一个ID结构体
        public IdStruct(uint time, int process, ushort value)
        {
            this.Process = process;
            this.Time = time;
            this.Value = value;
        }

        // 根据长整型创建一个ID结构体
        public IdStruct(long id)
        {
            ulong result = (ulong) id; 
            this.Value = (ushort) (result & ushort.MaxValue);
            result >>= 16;
            this.Process = (int) (result & IdGenerater.Mask18bit);
            result >>= 18;
            this.Time = (uint) result;
        }

        // 重写ToString方法，用于返回ID的字符串表示
        public override string ToString()
        {
            return $"process: {this.Process}, time: {this.Time}, value: {this.Value}";
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct InstanceIdStruct
    {
        public uint Time;   // 当年开始的tick 28bit，表示从当年开始的秒数，最多可用8.7年
        public int Process; // 18bit，表示进程号，最多可有262144个进程
        public uint Value;  // 18bit，表示每秒每个进程生成的实例ID数量，最多可有262144个

        // 定义一个方法，用于将实例ID转换为长整型
        public long ToLong()
        {
            ulong result = 0;
            result |= this.Value;
            result |= (ulong)this.Process << 18;
            result |= (ulong) this.Time << 36;
            return (long) result;
        }

        // 根据长整型创建一个实例ID结构体
        public InstanceIdStruct(long id)
        {
            ulong result = (ulong) id;
            this.Value = (uint)(result & IdGenerater.Mask18bit);
            result >>= 18;
            this.Process = (int)(result & IdGenerater.Mask18bit);
            result >>= 18;
            this.Time = (uint)result;
        }

        // 根据时间、进程和值创建一个实例ID结构体
        public InstanceIdStruct(uint time, int process, uint value)
        {
            this.Time = time;
            this.Process = process;
            this.Value = value;
        }

        // 给SceneId使用，只包含进程和值两个字段
        public InstanceIdStruct(int process, uint value)
        {
            this.Time = 0;
            this.Process = process;
            this.Value = value;
        }

        // 重写ToString方法，用于返回实例ID的字符串表示
        public override string ToString()
        {
            return $"process: {this.Process}, value: {this.Value} time: {this.Time}";
        }
    }
    
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct UnitIdStruct
    {
        public uint Time;        // 30bit，表示从2020年开始的秒数，最多可用34年
        public ushort Zone;      // 10bit，表示区域号，最多可有1024个区域
        public byte ProcessMode; // 8bit  Process % 256，表示进程号对256取余的结果，一个区域最多256个进程
        public ushort Value;     // 16bit 表示每秒每个进程生成的Unit数量，最多可有65536个

        // 将UnitID转换为长整型
        public long ToLong()
        {
            ulong result = 0;
            result |= this.Value;
            result |= (uint)this.ProcessMode << 16;
            result |= (ulong) this.Zone << 24;
            result |= (ulong) this.Time << 34;
            return (long) result;
        }

        // 根据区域、进程、时间和值创建一个UnitID结构体
        public UnitIdStruct(int zone, int process, uint time, ushort value)
        {
            this.Time = time;
            this.ProcessMode = (byte)(process % 256);
            this.Value = value;
            this.Zone = (ushort)zone;
        }

        // 根据长整型创建一个UnitID结构体
        public UnitIdStruct(long id)
        {
            ulong result = (ulong) id;
            this.Value = (ushort)(result & ushort.MaxValue);
            result >>= 16;
            this.ProcessMode = (byte)(result & byte.MaxValue);
            result >>= 8;
            this.Zone = (ushort)(result & 0x03ff);
            result >>= 10;
            this.Time = (uint)result;
        }

        // 重写ToString方法，用于返回UnitID的字符串表示
        public override string ToString()
        {
            return $"ProcessMode: {this.ProcessMode}, value: {this.Value} time: {this.Time}";
        }

        // 从UnitID中获取区域号
        public static int GetUnitZone(long unitId)
        {
            int v = (int) ((unitId >> 24) & 0x03ff); // 取出10bit
            return v;
        }
    }

    // 定义一个类，用于生成不同类型的ID
    public class IdGenerater: Singleton<IdGenerater>
    {
        public const int Mask18bit = 0x03ffff;   // 定义一个常量，用于表示18位的掩码

        public const int MaxZone = 1024;         // 定义一个常量，用于表示最大的区域数

        private long epoch2020;                  // 用于存储2020年开始的毫秒数
        private ushort value;                    // 用于存储每秒每个进程生成的ID数量
        private uint lastIdTime;                 // 用于存储上一次生成ID的时间

        private long epochThisYear;              // 用于存储当年开始的毫秒数
        private uint instanceIdValue;            // 用于存储每秒每个进程生成的实例ID数量
        private uint lastInstanceIdTime;         // 用于存储上一次生成实例ID的时间

        private ushort unitIdValue;              // 用于存储每秒每个进程生成的Unit数量
        private uint lastUnitIdTime;             // 用于存储上一次生成UnitID的时间

        public IdGenerater()
        {
            // 计算1970年开始的毫秒数
            long epoch1970tick = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).Ticks / 10000;

            // 计算2020年开始的毫秒数
            this.epoch2020 = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc).Ticks / 10000 - epoch1970tick;

            // 计算当年开始的毫秒数
            this.epochThisYear = new DateTime(DateTime.Now.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc).Ticks / 10000 - epoch1970tick;

            // 获取当年开始到现在的秒数，并赋值给lastInstanceIdTime变量
            this.lastInstanceIdTime = TimeSinceThisYear();
            if (this.lastInstanceIdTime <= 0)
            {
                Log.Warning($"lastInstanceIdTime less than 0: {this.lastInstanceIdTime}");

                // 将lastInstanceIdTime设为1，避免为0
                this.lastInstanceIdTime = 1;
            }

            // 获取2020年开始到现在的秒数，并赋值给lastIdTime变量
            this.lastIdTime = TimeSince2020();
            if (this.lastIdTime <= 0)
            {
                Log.Warning($"lastIdTime less than 0: {this.lastIdTime}");

                // 将lastIdTime设为1，避免为0
                this.lastIdTime = 1;
            }

            // 获取2020年开始到现在的秒数，并赋值给lastUnitIdTime变量
            this.lastUnitIdTime = TimeSince2020();
            if (this.lastUnitIdTime <= 0)
            {
                Log.Warning($"lastUnitIdTime less than 0: {this.lastUnitIdTime}");

                // 将lastUnitIdTime设为1，避免为0
                this.lastUnitIdTime = 1;
            }
        }

        // 计算从2020年开始到当前帧时间的秒数
        private uint TimeSince2020()
        {
            uint a = (uint)((TimeInfo.Instance.FrameTime - this.epoch2020) / 1000);
            return a;
        }
        
        // 计算从当年开始到当前帧时间的秒数
        private uint TimeSinceThisYear()
        {
            uint a = (uint)((TimeInfo.Instance.FrameTime - this.epochThisYear) / 1000);
            return a;
        }

        // 用于生成实例ID
        public long GenerateInstanceId()
        {
            uint time = TimeSinceThisYear();

            if (time > this.lastInstanceIdTime)
            {
                // 如果当前时间大于上一次生成实例ID的时间

                // 更新上一次生成实例ID的时间
                this.lastInstanceIdTime = time;

                // 将每秒每个进程生成的实例ID数量重置为0
                this.instanceIdValue = 0;
            }
            else
            {
                // 将每秒每个进程生成的实例ID数量加1
                ++this.instanceIdValue;
                
                if (this.instanceIdValue > IdGenerater.Mask18bit - 1) // 18bit
                {
                    // 如果每秒每个进程生成的实例ID数量超过了18位的最大值
                    ++this.lastInstanceIdTime; // 借用下一秒

                    // 将每秒每个进程生成的实例ID数量重置为0
                    this.instanceIdValue = 0;

                    Log.Error($"instanceid count per sec overflow: {time} {this.lastInstanceIdTime}");
                }
            }

            // 根据时间、进程和值创建一个实例ID结构体
            InstanceIdStruct instanceIdStruct = new InstanceIdStruct(this.lastInstanceIdTime, Options.Instance.Process, this.instanceIdValue);
            return instanceIdStruct.ToLong();
        }

        // 定义一个方法，用于生成ID
        public long GenerateId()
        {
            // 获取2020年开始到现在的秒数
            uint time = TimeSince2020();

            if (time > this.lastIdTime)
            {
                // 如果当前时间大于上一次生成ID的时间

                // 更新上一次生成ID的时间
                this.lastIdTime = time;

                // 将每秒每个进程生成的ID数量重置为0
                this.value = 0;
            }
            else
            {
                // 如果当前时间等于或小于上一次生成ID的时间

                // 将每秒每个进程生成的ID数量加1
                ++this.value;
                
                if (value > ushort.MaxValue - 1)
                {
                    // 如果每秒每个进程生成的ID数量超过了16位的最大值

                    // 将每秒每个进程生成的ID数量重置为0
                    this.value = 0;
                    ++this.lastIdTime; // 借用下一秒
                    Log.Error($"id count per sec overflow: {time} {this.lastIdTime}");
                }
            }

            // 根据时间、进程和值创建一个ID结构体
            IdStruct idStruct = new IdStruct(this.lastIdTime, Options.Instance.Process, value);
            return idStruct.ToLong();
        }

        // 定义一个方法，用于生成UnitID
        public long GenerateUnitId(int zone)
        {
            if (zone > MaxZone)
            {
                // 如果区域号大于最大区域数，说明出错了
                throw new Exception($"zone > MaxZone: {zone}");
            }

            // 获取2020年开始到现在的秒数
            uint time = TimeSince2020();

            if (time > this.lastUnitIdTime)
            {
                // 如果当前时间大于上一次生成UnitID的时间
                this.lastUnitIdTime = time;

                // 将每秒每个进程生成的Unit数量重置为0
                this.unitIdValue = 0;
            }
            else
            {
                // 如果当前时间等于或小于上一次生成UnitID的时间

                // 将每秒每个进程生成的Unit数量加1
                ++this.unitIdValue;
                
                if (this.unitIdValue > ushort.MaxValue - 1)
                {
                    // 如果每秒每个进程生成的Unit数量超过了16位的最大值

                    this.unitIdValue = 0;
                    ++this.lastUnitIdTime; // 借用下一秒
                    Log.Error($"unitid count per sec overflow: {time} {this.lastUnitIdTime}");
                }
            }

            // 创建一个UnitIdStruct结构体
            UnitIdStruct unitIdStruct = new UnitIdStruct(zone, Options.Instance.Process, this.lastUnitIdTime, this.unitIdValue);
            return unitIdStruct.ToLong();
        }
    }
}