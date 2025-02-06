using System;

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
            StartAsync().NoContext();
        }
        
        private static async ETTask StartAsync()
        {
            WinPeriod.Init();

            // 注册Mongo type
            MongoRegister.Init();
            
            MemoryPackRegister.Init();
            
            // 注册Entity序列化器
            EntitySerializeRegister.Init();

            World.Instance.AddSingleton<SceneTypeSingleton, Type>(typeof(SceneType));
            World.Instance.AddSingleton<ObjectPool>();
            World.Instance.AddSingleton<IdGenerater>();
            World.Instance.AddSingleton<OpcodeType>();
            
            World.Instance.AddSingleton<MessageQueue>();
            World.Instance.AddSingleton<NetServices>();
            
            LogMsg logMsg = World.Instance.AddSingleton<LogMsg>();
            logMsg.AddIgnore(LoginOuter.C2G_Ping);
            logMsg.AddIgnore(LoginOuter.G2C_Ping);
            
            // 创建需要reload的code singleton
            CodeTypes.Instance.CodeProcess();
            
            await World.Instance.AddSingleton<ConfigLoader>().LoadAsync();
            
            await FiberManager.Instance.Create(SchedulerType.Main, SceneType.Main, 0, SceneType.Main, "");
        }
    }
}