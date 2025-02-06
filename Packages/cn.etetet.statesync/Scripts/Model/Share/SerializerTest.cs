using MemoryPack;

namespace ET
{
    [MemoryPackable]
    [ComponentOf(typeof(Scene))]
    public partial class AA : Entity, IAwake
    {
    }

    [MemoryPackable]
    [ComponentOf(typeof(AA))]
    public partial class BB : Entity, IAwake, ISerializeToEntity
    {
        [MemoryPackInclude]
        public int B { get; set; }
    }

    [MemoryPackable]
    [ComponentOf(typeof(AA))]
    public partial class CC : Entity, IAwake //, ISerializeToEntity
    {
        [MemoryPackInclude]
        public int C { get; set; }
    }
}