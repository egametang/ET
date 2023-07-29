﻿using System.Collections.Generic;

namespace ET
{
    [EntitySystemOf(typeof(CoroutineLockQueueType))]
    [FriendOf(typeof(CoroutineLockQueueType))]
    public static partial class CoroutineLockQueueTypeSystem
    {
        [EntitySystem]
        private static void Awake(this CoroutineLockQueueType self)
        {
        }
        
        public static CoroutineLockQueue Get(this CoroutineLockQueueType self, long key)
        {
            return self.GetChild<CoroutineLockQueue>(key);
        }

        public static CoroutineLockQueue New(this CoroutineLockQueueType self, long key)
        {
            CoroutineLockQueue queue = self.AddChildWithId<CoroutineLockQueue, int>(key, (int)self.Id, true);
            return queue;
        }

        public static void Remove(this CoroutineLockQueueType self, long key)
        {
            self.RemoveChild(key);
        }

        public static async ETTask<CoroutineLock> Wait(this CoroutineLockQueueType self, long key, int time)
        {
            CoroutineLockQueue queue = self.Get(key) ?? self.New(key);
            return await queue.Wait(time);
        }

        public static void Notify(this CoroutineLockQueueType self, long key, int level)
        {
            CoroutineLockQueue queue = self.Get(key);
            if (queue == null)
            {
                return;
            }
            
            if (queue.Count == 0)
            {
                self.Remove(key);
            }

            queue.Notify(level);
        }
    }
    
    [ChildOf(typeof(CoroutineLockComponent))]
    public class CoroutineLockQueueType: Entity, IAwake
    {
    }
}