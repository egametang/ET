using System;
using System.Collections.Generic;

namespace ET
{
    public class MonoPool : IDisposable
    {
        /// <summary>
        /// 检查释放间隔（毫秒）
        /// </summary>
        public int ReleaseInterval { get; private set; } = 3000;
        /// <summary>
        /// 下次释放时间（毫秒）
        /// </summary>
        public long ReleaseNextRunTime { get; private set; }
        private readonly Dictionary<Type, Queue<object>> pool = new Dictionary<Type, Queue<object>>();

        public static MonoPool Instance = new MonoPool();

        /// <summary>
        /// 类对象在池中的常驻数量
        /// </summary>
        public Dictionary<Type, byte> ClassObjectCount { get; private set; } = new Dictionary<Type, byte>();

#if UNITY_EDITOR
        /// <summary>
        /// 在监视面板显示的信息
        /// </summary>
        public Dictionary<Type, int> InspectorDic = new Dictionary<Type, int>();
#endif

        private MonoPool()
        {
            ReleaseNextRunTime = TimeInfo.Instance.FrameTime;
        }

        /// <summary>
        /// 设置类常驻数量
        /// </summary>
        public void SetResideCount<T>(byte count) where T : class
        {
            ClassObjectCount[typeof(T)] = count;
        }

        public object Fetch(Type type)
        {
            Queue<object> queue = null;
            if (!pool.TryGetValue(type, out queue))
            {
                return Activator.CreateInstance(type);
            }

            if (queue.Count == 0)
            {
                return Activator.CreateInstance(type);
            }
#if UNITY_EDITOR
            if (InspectorDic.ContainsKey(type))
            {
                InspectorDic[type]--;
            }
            else
            {
                InspectorDic[type] = 0;
            }
#endif
            return queue.Dequeue();
        }

        public T Fetch<T>() where T : class
        {
            return Fetch(typeof(T)) as T;
        }

        public void Recycle(object obj)
        {
            Type type = obj.GetType();
            Queue<object> queue = null;
            if (!pool.TryGetValue(type, out queue))
            {
                queue = new Queue<object>();
                pool.Add(type, queue);
            }
#if UNITY_EDITOR
            if (InspectorDic.ContainsKey(type))
            {
                InspectorDic[type]++;
            }
            else
            {
                InspectorDic[type] = 1;
            }
#endif
            queue.Enqueue(obj);
        }

        public void Update()
        {
            if (TimeInfo.Instance.FrameTime > ReleaseNextRunTime + ReleaseInterval)
            {
                ReleaseNextRunTime = TimeInfo.Instance.FrameTime;
                CheckRelease();
            }
        }

        private void CheckRelease()
        {
            int queueCount = 0;

            var enumerator = pool.GetEnumerator();
            while (enumerator.MoveNext())
            {
                Type type = enumerator.Current.Key;
                Queue<object> queue = pool[type];
                queueCount = queue.Count;

                // 用户释放的时候 判断
                byte resideCount = 0;
                ClassObjectCount.TryGetValue(type, out resideCount);
                while (queueCount > resideCount)
                {
                    // 队列中有可释放的对象
                    queueCount--;
                    object obj = queue.Dequeue(); // 从队列中取出一个 这个对象没有任何引用，就变成了野指针 等待GC回收
#if UNITY_EDITOR
                    InspectorDic[type]--;
#endif
                }

                if (queueCount == 0)
                {
#if UNITY_EDITOR
                    if (type != null)
                    {
                        InspectorDic.Remove(type);
                    }
#endif
                }
            }

            GC.Collect();
        }

        public void Dispose()
        {
            this.pool.Clear();
        }
    }
}