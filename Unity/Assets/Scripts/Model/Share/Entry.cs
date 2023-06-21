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
            World.Instance.AddSingleton<WorldActor>();
            World.Instance.AddSingleton<ActorQueue>();
            World.Instance.AddSingleton<EntitySystemSingleton>();
            World.Instance.AddSingleton<LSEntitySystemSingleton>();
            World.Instance.AddSingleton<MessageDispatcherComponent>();
            World.Instance.AddSingleton<NumericWatcherComponent>();
            World.Instance.AddSingleton<AIDispatcherComponent>();
            World.Instance.AddSingleton<VProcessManager>();
            VProcessManager.MainThreadScheduler mainThreadScheduler = World.Instance.AddSingleton<VProcessManager.MainThreadScheduler>();

            int vProcessId = VProcessManager.Instance.Create(0, SceneType.Main);
            mainThreadScheduler.Add(vProcessId);
			
            // 发送消息
            ActorQueue.Instance.Send(new ActorId(Options.Instance.Process, vProcessId, 1), null);


            VProcess vProcess = VProcess.Instance;
            
            vProcess.AddComponent<MainThreadSynchronizationContext>();
            vProcess.AddComponent<TimerComponent>();
            vProcess.AddComponent<CoroutineLockComponent>();
            
            vProcess.AddComponent<NetServices>();

            await World.Instance.AddSingleton<ConfigComponent>().LoadAsync();

            await EventSystem.Instance.PublishAsync(vProcess, new EventType.EntryEvent1());
            await EventSystem.Instance.PublishAsync(vProcess, new EventType.EntryEvent2());
            await EventSystem.Instance.PublishAsync(vProcess, new EventType.EntryEvent3());
        }
    }
}