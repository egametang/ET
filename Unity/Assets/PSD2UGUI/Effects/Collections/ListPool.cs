using System.Collections;
using System.Collections.Generic;

namespace UGUI.Collections
{
    internal static class ListPool<T>
    {
        private static readonly Pool<List<T>> _listPool = new Pool<List<T>>(null, l => l.Clear());

        public static List<T> Get()
        {
            return _listPool.Get();
        }

        public static void Recycle(List<T> element)
        {
            _listPool.Recycle(element);
        }
    }
}
