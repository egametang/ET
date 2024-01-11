using System.Collections.Generic;

namespace YIUIFramework
{
    /// <summary>
    /// 堆栈池
    /// </summary>
    public class StackPool<T>
    {
        private static Stack<Stack<T>> g_pool = new Stack<Stack<T>>(5);

        public static Stack<T> Get()
        {
            lock (g_pool)
            {
                return g_pool.Count == 0
                    ? new Stack<T>()
                    : g_pool.Pop();
            }
        }

        public static void Put(Stack<T> list)
        {
            list.Clear();
            lock (g_pool)
            {
                g_pool.Push(list);
            }
        }

        public static T[] PutAndToArray(Stack<T> list)
        {
            var t = list.ToArray();
            Put(list);
            return t;
        }
    }
}