using System.Collections.Generic;
using MemoryPack;

namespace ET
{
    [MemoryPackable]
    public class SaveData
    {
        [MemoryPackOrder(1)]
        public List<OneFrameMessages> MessagesList = new();
        
        [MemoryPackOrder(2)]
        public List<byte[]> LSWorlds = new();
    }
}