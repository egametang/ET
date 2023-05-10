using System.Collections.Generic;
using MemoryPack;

namespace ET
{
    [MemoryPackable]
    public partial class SaveData
    {
        [MemoryPackOrder(1)]
        public List<OneFrameInputs> MessagesList = new();
        
        [MemoryPackOrder(2)]
        public List<byte[]> LSWorlds = new();
    }
}