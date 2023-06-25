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
            
            World.Instance.AddSingleton<OpcodeType>();
            World.Instance.AddSingleton<IdValueGenerater>();
            World.Instance.AddSingleton<ObjectPool>();
            World.Instance.AddSingleton<ActorMessageQueue>();
            World.Instance.AddSingleton<EntitySystemSingleton>();
            World.Instance.AddSingleton<LSEntitySystemSingleton>();
            World.Instance.AddSingleton<MessageDispatcherComponent>();
            World.Instance.AddSingleton<NumericWatcherComponent>();
            World.Instance.AddSingleton<AIDispatcherComponent>();
            World.Instance.AddSingleton<ActorMessageDispatcherComponent>();
            World.Instance.AddSingleton<FiberManager>();
            FiberManager.MainThreadScheduler mainThreadScheduler = World.Instance.AddSingleton<FiberManager.MainThreadScheduler>();

            int fiberId = FiberManager.Instance.Create(0, SceneType.Main);
            mainThreadScheduler.Add(fiberId);
			
            // 发送消息
            ActorMessageQueue.Instance.Send(new ActorId(Options.Instance.Process, fiberId, 1), null);


            Fiber fiber = Fiber.Instance;
            
            fiber.AddComponent<MainThreadSynchronizationContext>();
            fiber.AddComponent<TimerComponent>();
            fiber.AddComponent<CoroutineLockComponent>();
            
            fiber.AddComponent<NetServices>();

            await World.Instance.AddSingleton<ConfigComponent>().LoadAsync();

            await EventSystem.Instance.PublishAsync(fiber, new EventType.EntryEvent1());
            await EventSystem.Instance.PublishAsync(fiber, new EventType.EntryEvent2());
            await EventSystem.Instance.PublishAsync(fiber, new EventType.EntryEvent3());
        }
    }
}