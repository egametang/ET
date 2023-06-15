using System.IO;
using MemoryPack;

namespace ET
{
    namespace EventType
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
            
            MongoHelper.RegisterStruct<LSInput>();
            MongoHelper.Register();

            VProcess vProcess = VProcess.Instance;
            
            vProcess.AddSingleton<MainThreadSynchronizationContext>();
            vProcess.AddSingleton<TimeInfo>();
            vProcess.AddSingleton<IdGenerater>();
            vProcess.AddSingleton<TimerComponent>();
            vProcess.AddSingleton<CoroutineLockComponent>();
            vProcess.AddSingleton<EntitySystemSingleton>();
            vProcess.AddSingleton<LSEntitySystemSington>();

            vProcess.AddSingleton<NetServices>();
            vProcess.AddSingleton<Root>();

            await World.Instance.AddSingleton<ConfigComponent>().LoadAsync();

            await EventSystem.Instance.PublishAsync(Root.Instance.Scene, new EventType.EntryEvent1());
            await EventSystem.Instance.PublishAsync(Root.Instance.Scene, new EventType.EntryEvent2());
            await EventSystem.Instance.PublishAsync(Root.Instance.Scene, new EventType.EntryEvent3());
        }
    }
}