using System;
using System.Collections.Generic;

namespace ET
{
    public static class ForeachHelper
    {
        public static void Foreach<T, K>(this Dictionary<T, K> dictionary, Action<T, K> action)
        {
            foreach (var kv in dictionary)
            {
                action(kv.Key, kv.Value);
            }
        }
        
        public static void Foreach<T>(this HashSet<T> hashSet, Action<T> action)
        {
            foreach (var v in hashSet)
            {
                action(v);
            }
        }
        
        public static void Foreach<T, K>(this SortedDictionary<T, K> sortedDictionary, Action<T, K> action)
        {
            foreach (var kv in sortedDictionary)
            {
                action(kv.Key, kv.Value);
            }
        }
        
        public static void ForEach<T, K>(this MultiMap<T, K> multiMap, Action<T, List<K>> action)
        {
            foreach (var kv in multiMap)
            {
                action(kv.Key, kv.Value);
            }
        }
        
        public static void ForEachFunc<T, K>(this MultiMap<T, K> multiMap, Func<T, List<K>, bool> func)
        {
            foreach (var kv in multiMap)
            {
                if (!func(kv.Key, kv.Value))
                {
                    break;
                }
            }
        }
        
        public static void ForEach<T, K>(this UnOrderMultiMap<T, K> multiMap, Action<T, List<K>> action)
        {
            foreach (var kv in multiMap)
            {
                action(kv.Key, kv.Value);
            }
        }
        
        public static void ForEachFunc<T, K>(this UnOrderMultiMap<T, K> multiMap, Func<T, List<K>, bool> func)
        {
            foreach (var kv in multiMap)
            {
                if (!func(kv.Key, kv.Value))
                {
                    break;
                }
            }
        }
    }
}