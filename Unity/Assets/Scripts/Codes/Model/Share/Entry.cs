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
    
    [MemoryPackable]
    public partial class BB
    {
        public int a;
    }

    [MemoryPackable]
    public partial class AA
    {
        public int a;
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
            
            MongoHelper.Init();
            ProtobufHelper.Init();
            MemoryPackHelper.Init();

            Game.AddSingleton<NetServices>();
            Game.AddSingleton<Root>();
            await Game.AddSingleton<ConfigComponent>().LoadAsync();

            await EventSystem.Instance.PublishAsync(Root.Instance.Scene, new EventType.EntryEvent1());
            await EventSystem.Instance.PublishAsync(Root.Instance.Scene, new EventType.EntryEvent2());
            await EventSystem.Instance.PublishAsync(Root.Instance.Scene, new EventType.EntryEvent3());
        }
    }
}