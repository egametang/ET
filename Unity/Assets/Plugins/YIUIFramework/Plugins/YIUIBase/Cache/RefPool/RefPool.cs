using System.Collections.Generic;
using System;
using UnityEngine;

namespace YIUIFramework
{
    /// <summary>
    /// 引用池
    /// </summary>
    public static partial class RefPool
    {
        private static readonly Dictionary<Type, RefCollection>
            s_RefCollections = new Dictionary<Type, RefCollection>();

        public static int Count
        {
            get
            {
                lock (s_RefCollections)
                {
                    return s_RefCollections.Count;
                }
            }
        }

        /// <summary>
        /// 清除所有引用池
        /// </summary>
        public static void ClearAll()
        {
            lock (s_RefCollections)
            {
                foreach (KeyValuePair<Type, RefCollection> refCollection in s_RefCollections)
                {
                    refCollection.Value.RemoveAll();
                }

                s_RefCollections.Clear();
            }
        }

        /// <summary>
        /// 从引用池获取引用
        /// </summary>
        /// <typeparam name="T">引用类型</typeparam>
        /// <returns>引用</returns>
        public static T Get<T>() where T : class, IRefPool, new()
        {
            return GetRefCollection(typeof(T))?.Get<T>();
        }

        /// <summary>
        /// 从引用池获取引用
        /// </summary>
        /// <param name="refreceType">引用类型</param>
        /// <returns>引用</returns>
        public static IRefPool Get(Type refreceType)
        {
            return GetRefCollection(refreceType)?.Get();
        }

        /// <summary>
        /// 回收引用
        /// </summary>
        public static bool Put(IRefPool iRef)
        {
            if (iRef == null)
            {
                Debug.LogError("refType == null 无效 请检查");
                return false;
            }

            var  collection = GetRefCollection(iRef.GetType());
            return collection != null && collection.Put(iRef);
        }

        /// <summary>
        /// 向引用池中追加指定数量的引用
        /// </summary>
        /// <typeparam name="T">引用类型</typeparam>
        /// <param name="count">追加数量</param>
        public static void Add<T>(int count) where T : class, IRefPool, new()
        {
            GetRefCollection(typeof(T))?.Add<T>(count);
        }

        /// <summary>
        /// 向引用池中追加指定数量的引用
        /// </summary>
        /// <param name="refType">引用类型</param>
        /// <param name="count">追加数量</param>
        public static void Add(Type refType, int count)
        {
            GetRefCollection(refType)?.Add(count);
        }

        /// <summary>
        /// 从引用池中移除指定数量的引用
        /// </summary>
        /// <typeparam name="T">引用类型</typeparam>
        /// <param name="count">移除数量</param>
        public static void Remove<T>(int count) where T : class, IRefPool, new()
        {
            GetRefCollection(typeof(T),true)?.Remove(count);
        }

        /// <summary>
        /// 从引用池中移除指定数量的引用
        /// </summary>
        /// <param name="refType">引用类型</param>
        /// <param name="count">移除数量</param>
        public static void Remove(Type refType, int count)
        {
            GetRefCollection(refType,true)?.Remove(count);
        }

        /// <summary>
        /// 从引用池中移除所有的引用
        /// </summary>
        /// <typeparam name="T">引用类型</typeparam>
        public static void RemoveAll<T>() where T : class, IRefPool, new()
        {
            GetRefCollection(typeof(T),true)?.RemoveAll();
        }

        /// <summary>
        /// 从引用池中移除所有的引用
        /// </summary>
        /// <param name="refType">引用类型</param>
        public static void RemoveAll(Type refType)
        {
            GetRefCollection(refType,true)?.RemoveAll();
        }

        private static bool InternalCheckRefType(Type refType)
        {
            if (refType == null)
            {
                Debug.LogError("refType == null 无效 请检查");
                return false;
            }

            if (!refType.IsClass || refType.IsAbstract)
            {
                Debug.LogError("引用类型不是非抽象类类型");
                return false;
            }

            if (!typeof(IRefPool).IsAssignableFrom(refType))
            {
                Debug.LogError($"引用类型'{refType.FullName}'无效。");
                return false;
            }

            return true;
        }

        private static RefCollection GetRefCollection(Type refType, bool allowNull = false)
        {
            if (refType == null)
            {
                Debug.LogError("refType == null 无效 请检查");
                return null;
            }

            RefCollection refCollection = null;

            lock (s_RefCollections)
            {
                if (!s_RefCollections.TryGetValue(refType, out refCollection))
                {
                    if (allowNull)
                    {
                        return null;
                    }
                    
                    if (!InternalCheckRefType(refType))
                    {
                        return null;
                    }

                    refCollection = new RefCollection(refType);
                    s_RefCollections.Add(refType, refCollection);
                }
            }

            return refCollection;
        }
    }
}