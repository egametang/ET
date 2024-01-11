using System.Collections.Generic;

namespace YIUIFramework
{
    /// <summary>
    /// 字典的池
    /// </summary>
    public class DictionaryPool<TKey, TValue>
    {
        private static Stack<Dictionary<TKey, TValue>> g_pool = new Stack<Dictionary<TKey, TValue>>();

        public static Dictionary<TKey, TValue> Get()
        {
            lock (g_pool)
            {
                return g_pool.Count == 0
                    ? new Dictionary<TKey, TValue>()
                    : g_pool.Pop();
            }
        }

        public static void Put(Dictionary<TKey, TValue> map)
        {
            map.Clear();
            lock (g_pool)
            {
                g_pool.Push(map);
            }
        }
    }
}