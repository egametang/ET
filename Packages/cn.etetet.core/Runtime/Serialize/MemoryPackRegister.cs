using MemoryPack;

namespace ET
{
    public static class MemoryPackRegister
    {
        public static void Init()
        {
            MemoryPackFormatterProvider.Register(new MemoryPackChildrenCollectionFormatter());
            MemoryPackFormatterProvider.Register(new MemoryPackComponentsCollectionFormatter());
        }
    }
}