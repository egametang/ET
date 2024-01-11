using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;

namespace YIUIFramework
{
    /// <summary>
    /// <see cref="Array"/>.
    /// </summary>
    public static class ArrayExtensions
    {
        /// <summary>
        /// 洗牌
        /// </summary>
        public static void Shuffle<T>(this T[] array)
        {
            for (int i = 0; i < array.Length; ++i)
            {
                var randIdx = Random.Range(0, array.Length);
                (array[randIdx], array[i]) = (array[i], array[randIdx]);
            }
        }

        /// <summary>
        /// 删除重复
        /// </summary>
        public static T[] RemoveDuplicate<T>(this T[] array)
        {
            var lookup = new HashSet<T>();
            foreach (var i in array)
            {
                if (!lookup.Contains(i))
                {
                    lookup.Add(i);
                }
            }

            var newArray = new T[lookup.Count];
            int index    = 0;
            foreach (var i in lookup)
            {
                newArray[index++] = i;
            }

            return newArray;
        }

        /// <summary>
        /// 将数组强制转换为子类
        /// </summary>
        public static U[] Cast<T, U>(this T[] array)
            where U : class, T
            where T : class
        {
            return Array.ConvertAll<T, U>(array, input => input as U);
        }
    }
}