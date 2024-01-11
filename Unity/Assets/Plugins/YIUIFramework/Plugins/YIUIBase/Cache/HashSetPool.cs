using System.Collections.Generic;

namespace YIUIFramework
{
    /// <summary>
    /// HashSet的池
    /// </summary>
    public class HashSetPool<T>
    {
        private static Stack<HashSet<T>> g_pool = new Stack<HashSet<T>>(5);

        public static HashSet<T> Get()
        {
            lock (g_pool)
            {
                return g_pool.Count == 0
                    ? new HashSet<T>()
                    : g_pool.Pop();
            }
        }

        /// <summary>
        /// 得到一个hashSet, 并帮你填充了data
        /// </summary>
        /// <param name="initData"></param>
        /// <returns></returns>
        public static HashSet<T> Get(T[] initData)
        {
            var map = Get();
            for (int i = 0; i < initData.Length; i++)
            {
                map.Add(initData[i]);
            }

            return map;
        }

        /// <summary>
        /// 得到一个hashSet, 并帮你填充了data
        /// </summary>
        /// <param name="initData"></param>
        /// <returns></returns>
        public static HashSet<T> Get(IList<T> initData)
        {
            var map = Get();
            for (int i = 0; i < initData.Count; i++)
            {
                map.Add(initData[i]);
            }

            return map;
        }

        public static void Put(HashSet<T> list)
        {
            list.Clear();
            lock (g_pool)
            {
                g_pool.Push(list);
            }
        }
    }
}