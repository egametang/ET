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
    }
}