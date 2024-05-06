using System;

namespace DotRecast.Core
{
    public static class RcImmutableArray
    {
        public static RcImmutableArray<T> Create<T>()
        {
            return RcImmutableArray<T>.Empty;
        }

        public static RcImmutableArray<T> Create<T>(T item1)
        {
            T[] array = new[] { item1 };
            return new RcImmutableArray<T>(array);
        }

        public static RcImmutableArray<T> Create<T>(T item1, T item2)
        {
            T[] array = new[] { item1, item2 };
            return new RcImmutableArray<T>(array);
        }

        public static RcImmutableArray<T> Create<T>(T item1, T item2, T item3)
        {
            T[] array = new[] { item1, item2, item3 };
            return new RcImmutableArray<T>(array);
        }

        public static RcImmutableArray<T> Create<T>(T item1, T item2, T item3, T item4)
        {
            T[] array = new[] { item1, item2, item3, item4 };
            return new RcImmutableArray<T>(array);
        }

        public static RcImmutableArray<T> Create<T>(params T[] items)
        {
            if (items == null || items.Length == 0)
            {
                return RcImmutableArray<T>.Empty;
            }

            var tmp = new T[items.Length];
            Array.Copy(items, tmp, items.Length);
            return new RcImmutableArray<T>(tmp);
        }
    }
}