using System;
using System.Collections.Generic;

namespace YIUIFramework
{
    /// <summary>
    /// <see cref="Dictionary"/>.
    /// </summary>
    public static class DictionaryExtensions
    {
        /// <summary>
        /// 删除过滤器检查的所有值
        /// </summary>
        public static void RemoveAll<K, V>(
            this Dictionary<K, V> dic, Func<K, V, bool> filter)
        {
            var sweep = RemoveList<K>.SweepList;

            var itr = dic.GetEnumerator();
            while (itr.MoveNext())
            {
                var kv = itr.Current;
                if (filter(kv.Key, kv.Value))
                {
                    sweep.Add(kv.Key);
                }
            }

            for (int i = 0; i < sweep.Count; ++i)
            {
                dic.Remove(sweep[i]);
            }

            sweep.Clear();
        }

        private static class RemoveList<T>
        {
            private static List<T> sweepList = new List<T>();

            public static List<T> SweepList
            {
                get { return sweepList; }
            }
        }
    }
}