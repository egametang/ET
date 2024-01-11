using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace YIUIFramework
{
    public static class ListExt
    {
        /// <summary>
        /// 将列表转为字典
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TItem"></typeparam>
        /// <param name="list"></param>
        /// <param name="getKeyHandler"></param>
        /// <returns></returns>
        public static Dictionary<TKey, TItem> ToDictionary<TKey, TItem>(
            this IList<TItem> list, Func<TItem, TKey> getKeyHandler)
        {
            if (list == null)
            {
                return EmptyValue<Dictionary<TKey, TItem>>.Value;
            }

            var dic = new Dictionary<TKey, TItem>(list.Count);
            for (int i = 0; i < list.Count; i++)
            {
                dic[getKeyHandler(list[i])] = list[i];
            }

            return dic;
        }

        /// <summary>
        /// 排序自身
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="source"></param>
        /// <param name="keySelector"></param>
        /// <param name="isDescending"></param>
        public static void OrderSelf<TSource, TKey>(this IList<TSource> source, Func<TSource, TKey> keySelector,
                                                    bool                isDescending = false)
        {
            IOrderedEnumerable<TSource> result;
            if (isDescending)
            {
                result = source.OrderByDescending(keySelector);
            }
            else
            {
                result = source.OrderBy(keySelector);
            }

            int i = 0;
            foreach (TSource item in result)
            {
                source[i] = item;
                i++;
            }
        }

        /// <summary>
        /// 取出数据最后一个元素（取出后，这个元素就不在数据里）
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        public static T Pop<T>(this IList<T> list)
        {
            if (list.Count < 1)
            {
                return default;
            }

            var lastIndex = list.Count - 1;
            var lastItem  = list[lastIndex];
            list.RemoveAt(lastIndex);
            return lastItem;
        }

        /// <summary>
        /// 取出数据第一个元素（取出后，这个元素就不在数据里）
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        public static T Shift<T>(this IList<T> list)
        {
            if (list.Count < 1)
            {
                return default;
            }

            var firstItem = list[0];
            list.RemoveAt(0);
            return firstItem;
        }

        /// <summary>
        /// 在数组开头，添加一个或多个元素
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="inst"></param>
        /// <param name="items"></param>
        /// <returns></returns>
        public static T[] Unshift<T>(this IList<T> inst, params T[] items)
        {
            var addCount = items.Length;
            var newArr   = new T[addCount + inst.Count];
            for (int i = 0; i < addCount; i++)
            {
                newArr[i] = items[i];
            }

            for (int i = 0; i < inst.Count; i++)
            {
                newArr[addCount + i] = inst[i];
            }

            return newArr;
        }

        /// <summary>
        /// 用于IndexOf
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool EqualsUInt(uint a, uint b)
        {
            return a == b;
        }

        public static int IndexOf<T>(this IList<T> list,      T   value,
                                     int           start = 0, int count = 0, Func<T, T, bool> equalsFun = null)
        {
            count = count < 1
                ? list.Count
                : MathUtil.FixedByRange(count, 0, list.Count);

            if (equalsFun == null)
            {
                for (int i = start; i < count; i++)
                {
                    if (list[i].Equals(value))
                    {
                        return i;
                    }
                }
            }
            else
            {
                for (int i = start; i < count; i++)
                {
                    if (equalsFun(list[i], value))
                    {
                        return i;
                    }
                }
            }

            return -1;
        }

        /// <summary>
        /// 快速数组数据移除方法
        /// 当数组顺序不敏感时使用
        /// </summary>
        /// <param name="list"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static bool FastRemoveAt(this IList list, int index)
        {
            return FastRemoveAt(list, index, out _);
        }

        /// <summary>
        /// 快速数组数据移除方法
        /// 当数组顺序不敏感时使用
        /// </summary>
        /// <param name="list"></param>
        /// <param name="index"></param>
        /// <param name="curMoveItemIndex">被移动的对象现在所在的index, 当值小于零时表示没有被移动的物体</param>
        /// <returns></returns>
        public static bool FastRemoveAt(this IList list, int index, out int curMoveItemIndex)
        {
            int len = list.Count;
            if (len < 1 || index >= len)
            {
                curMoveItemIndex = -1;
                return false;
            }

            int    lastIndex = len - 1;
            object lastItem  = list[lastIndex];
            list.RemoveAt(lastIndex);
            if (index != lastIndex)
            {
                list[index]      = lastItem;
                curMoveItemIndex = index;
            }
            else
            {
                curMoveItemIndex = -1;
            }

            return true;
        }

        public static bool FastRemove(this IList list, object item)
        {
            var index = list.IndexOf(item);
            if (index < 0)
            {
                return false;
            }

            return FastRemoveAt(list, index, out _);
        }

        public static bool VerifyIndex(this IList list, int index, object item)
        {
            int len = list.Count;
            if (len < 1
             || index >= len
             || index < 0)
            {
                return false;
            }

            return list[index] == item;
        }

        public static bool SafeSetValue<T>(this IList<T> list, int index, T value)
        {
            if (null == list
             || index < 0
             || index >= list.Count)
            {
                return false;
            }

            list[index] = value;
            return true;
        }

        public static T SafeGetValue<T>(this IList<T> list, int index, T defValue = default(T))
        {
            if (list == null ||
                index >= list.Count)
            {
                return defValue;
            }

            return list[index];
        }

        public static T GetRnd<T>(this IList<T> list, Random random)
        {
            int len = list.Count;
            if (len < 1)
            {
                return default(T);
            }

            return list[random.Next(0, len)];
        }

        public static bool ContainsByUint(this uint[] list, uint value)
        {
            for (int i = 0; i < list.Length; i++)
            {
                if (list[i] == value)
                {
                    return true;
                }
            }

            return false;
        }
    }
}