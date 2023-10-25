﻿using System;

namespace ET.Server
{
    [EntitySystemOf(typeof(LockInfo))]
    [FriendOf(typeof(LockInfo))]
    public static partial class LockInfoSystem
    {
        [EntitySystem]
        private static void Awake(this LockInfo self, ActorId lockActorId, CoroutineLock coroutineLock)
        {
            self.CoroutineLock = coroutineLock;
            self.LockActorId = lockActorId;
        }
        
        [EntitySystem]
        private static void Destroy(this LockInfo self)
        {
            self.CoroutineLock.Dispose();
            self.LockActorId = default;
        }
    }
    

    [EntitySystemOf(typeof(LocationOneType))]
    [FriendOf(typeof(LocationOneType))]
    [FriendOf(typeof(LockInfo))]
    public static partial class LocationOneTypeSystem
    {
        [EntitySystem]
        private static void Awake(this LocationOneType self, int locationType)
        {
            self.LocationType = locationType;
        }
        
        public static async ETTask Add(this LocationOneType self, long key, ActorId instanceId)
        {
            int coroutineLockType = (self.LocationType << 16) | CoroutineLockType.Location;
            using (await self.Fiber().CoroutineLockComponent.Wait(coroutineLockType, key))
            {
                self.locations[key] = instanceId;
                Log.Info($"location add key: {key} instanceId: {instanceId}");
            }
        }

        public static async ETTask Remove(this LocationOneType self, long key)
        {
            int coroutineLockType = (self.LocationType << 16) | CoroutineLockType.Location;
            using (await self.Fiber().CoroutineLockComponent.Wait(coroutineLockType, key))
            {
                self.locations.Remove(key);
                Log.Info($"location remove key: {key}");
            }
        }

        public static async ETTask Lock(this LocationOneType self, long key, ActorId actorId, int time = 0)
        {
            int coroutineLockType = (self.LocationType << 16) | CoroutineLockType.Location;
            CoroutineLock coroutineLock = await self.Fiber().CoroutineLockComponent.Wait(coroutineLockType, key);

            LockInfo lockInfo = self.AddChild<LockInfo, ActorId, CoroutineLock>(actorId, coroutineLock);
            self.lockInfos.Add(key, lockInfo);

            Log.Info($"location lock key: {key} instanceId: {actorId}");

            if (time > 0)
            {
                async ETTask TimeWaitAsync()
                {
                    long lockInfoInstanceId = lockInfo.InstanceId;
                    await self.Fiber().TimerComponent.WaitAsync(time);
                    if (lockInfo.InstanceId != lockInfoInstanceId)
                    {
                        return;
                    }
                    Log.Info($"location timeout unlock key: {key} instanceId: {actorId} newInstanceId: {actorId}");
                    self.UnLock(key, actorId, actorId);
                }
                TimeWaitAsync().Coroutine();
            }
        }

        public static void UnLock(this LocationOneType self, long key, ActorId oldActorId, ActorId newInstanceId)
        {
            if (!self.lockInfos.TryGetValue(key, out LockInfo lockInfo))
            {
                Log.Error($"location unlock not found key: {key} {oldActorId}");
                return;
            }

            if (oldActorId != lockInfo.LockActorId)
            {
                Log.Error($"location unlock oldInstanceId is different: {key} {oldActorId}");
                return;
            }

            Log.Info($"location unlock key: {key} instanceId: {oldActorId} newInstanceId: {newInstanceId}");

            self.locations[key] = newInstanceId;

            self.lockInfos.Remove(key);

            // 解锁
            lockInfo.Dispose();
        }

        public static async ETTask<ActorId> Get(this LocationOneType self, long key)
        {
            int coroutineLockType = (self.LocationType << 16) | CoroutineLockType.Location;
            using (await self.Fiber().CoroutineLockComponent.Wait(coroutineLockType, key))
            {
                self.locations.TryGetValue(key, out ActorId actorId);
                Log.Info($"location get key: {key} actorId: {actorId}");
                return actorId;
            }
        }
    }

    [EntitySystemOf(typeof(LocationManagerComoponent))]
    [FriendOf(typeof (LocationManagerComoponent))]
    public static partial class LocationComoponentSystem
    {
        [EntitySystem]
        private static void Awake(this LocationManagerComoponent self)
        {
            for (int i = 0; i < self.LocationOneTypes.Length; ++i)
            {
                self.LocationOneTypes[i] = self.AddChild<LocationOneType, int>(i);
            }
        }
        
        public static LocationOneType Get(this LocationManagerComoponent self, int locationType)
        {
            return self.LocationOneTypes[locationType];
        }
    }
}