using System;
using System.Runtime.InteropServices;
using MemoryPack;

namespace ET
{
    [MemoryPackable]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public partial struct ActorId
    {
        public bool Equals(ActorId other)
        {
            return this.Process == other.Process && this.VProcess == other.VProcess && this.InstanceId == other.InstanceId;
        }

        public override bool Equals(object obj)
        {
            return obj is ActorId other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(this.Process, this.VProcess, this.InstanceId);
        }

        [MemoryPackOrder(0)]
        public short Process;
        [MemoryPackOrder(1)]
        public int VProcess;
        [MemoryPackOrder(2)]
        public long InstanceId;
        
        public ActorId(short process, int vProcess, long instanceId)
        {
            this.Process = process;
            this.VProcess = vProcess;
            this.InstanceId = instanceId;
        }

        public static bool operator ==(ActorId left, ActorId right)
        {
            return left.InstanceId == right.InstanceId && left.Process == right.Process && left.VProcess == right.VProcess;
        }

        public static bool operator !=(ActorId left, ActorId right)
        {
            return !(left == right);
        }
    }
}