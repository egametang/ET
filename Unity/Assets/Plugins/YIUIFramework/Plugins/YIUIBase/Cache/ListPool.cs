using System.Collections.Generic;

namespace YIUIFramework
{
    /// <summary>
    /// 各种列表的池
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static class ListPool<T>
    {
        private static Stack<List<T>> g_pool = new Stack<List<T>>(5);

        public static List<T> Get()
        {
            lock (g_pool)
            {
                return g_pool.Count == 0
                    ? new List<T>()
                    : g_pool.Pop();
            }
        }

        public static void Put(List<T> list)
        {
            list.Clear();
            lock (g_pool)
            {
                g_pool.Push(list);
            }
        }

        public static T[] PutAndToArray(List<T> list)
        {
            var t = list.ToArray();
            Put(list);
            return t;
        }
    }
}