using System;
using System.Collections.Generic;

namespace ET
{
    public static class Game
    {
        /// <summary>
        /// 定义一个私有静态只读字典，用来存储单例对象的类型和实例
        /// </summary>
        [StaticField]
        private static readonly Dictionary<Type, ISingleton> singletonTypes = new Dictionary<Type, ISingleton>();

        /// <summary>
        /// 定义一个私有静态只读栈，用来存储单例对象的顺序
        /// </summary>
        [StaticField]
        private static readonly Stack<ISingleton> singletons = new Stack<ISingleton>();

        /// <summary>
        /// 定义一个私有静态只读队列，用来存储需要更新的单例对象
        /// </summary>
        [StaticField]
        private static readonly Queue<ISingleton> updates = new Queue<ISingleton>();

        /// <summary>
        /// 定义一个私有静态只读队列，用来存储需要延迟更新的单例对象
        /// </summary>
        [StaticField]
        private static readonly Queue<ISingleton> lateUpdates = new Queue<ISingleton>();

        /// <summary>
        /// 定义一个私有静态只读队列，用来存储等待帧结束的任务
        /// </summary>
        [StaticField]
        private static readonly Queue<ETTask> frameFinishTask = new Queue<ETTask>();

        /// <summary>
        /// 用来创建和注册一个泛型参数T类型的单例对象，T必须继承自Singleton<T>并且有无参构造函数
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T AddSingleton<T>() where T: Singleton<T>, new()
        {
            T singleton = new T();
            AddSingleton(singleton);
            return singleton;
        }

        /// <summary>
        /// 用来注册一个已经创建好的单例对象
        /// </summary>
        /// <param name="singleton"></param>
        /// <exception cref="Exception"></exception>
        public static void AddSingleton(ISingleton singleton)
        {
            Type singletonType = singleton.GetType();
            if (singletonTypes.ContainsKey(singletonType))
            {
                // 抛出异常，表示已经存在该类型的单例对象
                throw new Exception($"already exist singleton: {singletonType.Name}");
            }

            // 否则将该类型和单例对象添加到字典中
            singletonTypes.Add(singletonType, singleton);

            // 将单例对象压入栈中
            singletons.Push(singleton);

            // 调用单例对象的Register方法，进行注册
            singleton.Register();

            // 如果单例对象实现了ISingletonAwake接口
            if (singleton is ISingletonAwake awake)
            {
                // 调用Awake方法，用来执行一些初始化逻辑
                awake.Awake();
            }

            // 如果单例对象实现了ISingletonUpdate接口
            if (singleton is ISingletonUpdate)
            {
                // 将单例对象入队到更新队列中
                updates.Enqueue(singleton);
            }

            // 如果单例对象实现了ISingletonLateUpdate接口
            if (singleton is ISingletonLateUpdate)
            {
                // 将单例对象入队到延迟更新队列中
                lateUpdates.Enqueue(singleton);
            }
        }

        /// <summary>
        /// 定义一个静态异步方法WaitFrameFinish，用来等待当前帧结束，并返回一个ETTask类型的任务
        /// </summary>
        /// <returns></returns>
        public static async ETTask WaitFrameFinish()
        {
            ETTask task = ETTask.Create(true);
            frameFinishTask.Enqueue(task);
            await task;
        }

        /// <summary>
        /// 用来执行更新队列中的所有单例对象
        /// </summary>
        public static void Update()
        {
            int count = updates.Count;
            while (count-- > 0)
            {
                // 从更新队列中取出一个单例对象
                ISingleton singleton = updates.Dequeue();

                if (singleton.IsDisposed())
                {
                    continue;
                }

                // 如果该单例对象不实现ISingletonUpdate接口，跳过本次循环
                if (singleton is not ISingletonUpdate update)
                {
                    continue;
                }

                // 将该单例对象重新加入到更新队列中
                updates.Enqueue(singleton);
                try
                {
                    // 调用该单例对象的Update方法
                    update.Update();
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }

        /// <summary>
        /// 用来执行延迟更新队列中的所有单例对象
        /// </summary>
        public static void LateUpdate()
        {
            int count = lateUpdates.Count;
            while (count-- > 0)
            {
                // 从延迟更新队列中取出一个单例对象
                ISingleton singleton = lateUpdates.Dequeue();
                
                if (singleton.IsDisposed())
                {
                    continue;
                }

                // 如果该单例对象不实现ISingletonLateUpdate接口，跳过本次循环
                if (singleton is not ISingletonLateUpdate lateUpdate)
                {
                    continue;
                }

                // 将该单例对象重新加入到延迟更新队列中
                lateUpdates.Enqueue(singleton);
                try
                {
                    // 调用该单例对象的LateUpdate方法
                    lateUpdate.LateUpdate();
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }

        /// <summary>
        /// 用来执行帧结束任务队列中的所有任务
        /// </summary>
        public static void FrameFinishUpdate()
        {
            while (frameFinishTask.Count > 0)
            {
                // 从帧结束任务队列中取出一个任务
                ETTask task = frameFinishTask.Dequeue();

                // 设置该任务的结果为成功
                task.SetResult();
            }
        }

        /// <summary>
        /// // 用来关闭所有的单例对象并清理相关数据结构
        /// </summary>
        public static void Close()
        {
            // 顺序反过来清理
            while (singletons.Count > 0)
            {
                ISingleton iSingleton = singletons.Pop();
                iSingleton.Destroy();
            }
            singletonTypes.Clear();
        }
    }
}