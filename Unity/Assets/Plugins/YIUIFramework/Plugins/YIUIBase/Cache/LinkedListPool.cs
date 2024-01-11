using System.Collections.Generic;

namespace YIUIFramework
{
    /*public static class ListPool<T>
    {
        private static readonly ObjectPool<List<T>> s_ListPool = new ObjectPool<List<T>>(null, l => l.Clear());

        public static List<T> Get()
        {
            return s_ListPool.Get();
        }

        public static void Release(List<T> toRelease)
        {
            s_ListPool.Release(toRelease);
        }
    }*/

    public static class LinkedListPool<T>
    {
        private static readonly ObjectPool<LinkedList<T>> s_ListPool =
            new ObjectPool<LinkedList<T>>(null, l => l.Clear());

        public static LinkedList<T> Get()
        {
            return s_ListPool.Get();
        }

        public static void Release(LinkedList<T> toRelease)
        {
            s_ListPool.Release(toRelease);
        }
    }
}