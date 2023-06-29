﻿using System.IO;
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

            int fiberId = FiberManager.Instance.Create(ConstFiberId.Main, SceneType.Main);
            mainThreadScheduler.Add(fiberId);

            await World.Instance.AddSingleton<ConfigComponent>().LoadAsync();
        }
    }
}