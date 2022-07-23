
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ET
{
    public static class MorphlingExtensions
    {
        public static bool IsNullOrEmpty(this string str)
        {
            return string.IsNullOrEmpty(str);
        }
        public static bool NotEmpty(this string str)
        {
            return !string.IsNullOrEmpty(str);
        }
        public static bool SavelyAdd<K, V>(this Dictionary<K, V> dic, K key, V value, bool replace = true)
        {
            if (replace)
            {
                dic[key] = value;
                return true;
            }
            if (dic.ContainsKey(key))
            {
                return false;
            }
            else
            {
                dic.Add(key, value);
                return true;
            }
        }
        public static bool SavelyRemove<K, V>(this Dictionary<K, V> dic, K key)
        {
            if (dic.ContainsKey(key))
            {
                dic.Remove(key);
                return true;
            }
            else
            {
                return false;
            }
        }
        #region 客户端
#if !NOT_UNITY
        public static T GetOrAddComponent<T>(this GameObject go) where T : Component
        {
            return go.GetComponent<T>() ?? go.AddComponent<T>();
        }
        public static Component GetOrAddComponent(this GameObject go, Type type)
        {
            return go.GetComponent(type) ?? go.AddComponent(type);
        }
#endif
        #endregion
    }
}
