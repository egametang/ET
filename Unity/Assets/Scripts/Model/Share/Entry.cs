using System;
using System.Collections.Generic;
using System.Reflection;
using MemoryPack;

namespace ET
{
    public struct EntryEvent1
    {
    }   
    
    public struct EntryEvent2
    {
    } 
    
    public struct EntryEvent3
    {
    }
    
    [MemoryPackable]
    [ComponentOf(typeof(Scene))]
    public partial class AA: Entity, IAwake
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
    
    public static class Entry
    {
        public static void Init()
        {
            
        }
        
        public static void Start()
        {
            StartAsync().Coroutine();
        }
        
        private static async ETTask StartAsync()
        {
            WinPeriod.Init();

            // 注册Mongo type
            MongoRegister.Init();
            
            MemoryPackFormatterProvider.Register(new MemoryPackSortedDictionaryFormatter<long, Entity>());
            
            // 注册Entity序列化器
            EntitySerializeRegister.Init();
            World.Instance.AddSingleton<IdGenerater>();
            World.Instance.AddSingleton<OpcodeType>();
            World.Instance.AddSingleton<ObjectPool>();
            World.Instance.AddSingleton<MessageQueue>();
            World.Instance.AddSingleton<NetServices>();
            World.Instance.AddSingleton<NavmeshComponent>();
            World.Instance.AddSingleton<LogMsg>();
            
            // 创建需要reload的code singleton
            CodeTypes.Instance.CreateCode();
            
            await World.Instance.AddSingleton<ConfigLoader>().LoadAsync();

            await FiberManager.Instance.Create(SchedulerType.Main, ConstFiberId.Main, 0, SceneType.Main, "");
        }
    }
}