using System.Collections.Generic;
using MemoryPack;

namespace ET
{
    [MemoryPackable]
    public partial class Record
    {
        [MemoryPackOrder(1)]
        public List<LockStepUnitInfo> UnitInfos;
        
        [MemoryPackOrder(2)]
        public List<OneFrameInputs> FrameInputs = new();
        
        [MemoryPackOrder(3)]
        public List<byte[]> Snapshots = new();
    }
}