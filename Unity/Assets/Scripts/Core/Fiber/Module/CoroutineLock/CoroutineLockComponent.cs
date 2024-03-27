using System;
using System.Collections.Generic;

namespace ET
{
    [EntitySystemOf(typeof(CoroutineLockComponent))]
    public static partial class CoroutineLockComponentSystem
    {
        [EntitySystem]
        public static void Awake(this CoroutineLockComponent self)
        {
        }
        
        [EntitySystem]
        public static void Update(this CoroutineLockComponent self)
        {
            // 循环过程中会有对象继续加入队列
            while (self.nextFrameRun.Count > 0)
            {
                (int coroutineLockType, long key, int count) = self.nextFrameRun.Dequeue();
                self.Notify(coroutineLockType, key, count);
            }
        }

        public static void RunNextCoroutine(this CoroutineLockComponent self, int coroutineLockType, long key, int level)
        {
            // 一个协程队列一帧处理超过100个,说明比较多了,打个warning,检查一下是否够正常
            if (level == 100)
            {
                Log.Warning($"too much coroutine level: {coroutineLockType} {key}");
            }

            self.nextFrameRun.Enqueue((coroutineLockType, key, level));
        }
        
        /// <summary>
        /// 等待方法执行完毕释放
        /// </summary>
        /// <param name="self"></param>
        /// <param name="coroutineLockType">组件的ChildId，自定义不同模块的锁</param>
        /// <param name="key">组件里的队列的Key，同一个Key的协程将排队处理，不同Key则各自执行</param>
        /// <param name="time">进入队列之后开始计时，超过这个时间就会抛出错误</param>
        /// <returns></returns>
        public static async ETTask<CoroutineLock> Wait(this CoroutineLockComponent self, int coroutineLockType, long key, int time = 60000)
        {
            CoroutineLockQueueType coroutineLockQueueType = self.GetChild<CoroutineLockQueueType>(coroutineLockType) ?? self.AddChildWithId<CoroutineLockQueueType>(coroutineLockType);
            return await coroutineLockQueueType.Wait(key, time);
        }

        private static void Notify(this CoroutineLockComponent self, int coroutineLockType, long key, int level)
        {
            CoroutineLockQueueType coroutineLockQueueType = self.GetChild<CoroutineLockQueueType>(coroutineLockType);
            if (coroutineLockQueueType == null)
            {
                return;
            }
            coroutineLockQueueType.Notify(key, level);
        }
    }
    
    [ComponentOf(typeof(Scene))]
    public class CoroutineLockComponent: Entity, IAwake, IScene, IUpdate
    {
        public Fiber Fiber { get; set; }
        public SceneType SceneType { get; set; }
        
        public readonly Queue<(int, long, int)> nextFrameRun = new();
    }
}