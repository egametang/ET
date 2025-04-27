using MemoryPack;
using System;
using mimalloc;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Serializers;

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
#if DOTNET
            // 尝试在DotNet使用mimalloc
            bool mimallocEnabled = true;
            try
            {
                _ = MiMalloc.mi_version();
            }
            catch
            {
                mimallocEnabled = false;
                Log.Info("mimalloc 不支持");
            }

            if (mimallocEnabled)
            {
                unsafe
                {
                    // 替换kcp的默认分配
                    KCP.ikcp_allocator(&MiMalloc.mi_malloc, &MiMalloc.mi_free);
                }
            }
#endif
            
            WinPeriod.Init();

            // 注册Mongo type
            MongoRegister.Init();
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