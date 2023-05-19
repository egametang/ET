// Animancer // https://kybernetik.com.au/animancer // Copyright 2020 Kybernetik //

//#define ANIMANCER_LOG_OBJECT_POOLING

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace Animancer
{
    /// <summary>Convenience methods for accessing <see cref="ObjectPool{T}"/>.</summary>
    /// https://kybernetik.com.au/animancer/api/Animancer/ObjectPool
    /// 
    public static class ObjectPool
    {
        /************************************************************************************************************************/

        /// <summary>
        /// Calls <see cref="ObjectPool{T}.Acquire"/> to get a spare item if there are any, or create a new one.
        /// </summary>
        /// <remarks>Remember to <see cref="Release{T}(T)"/> it when you are done.</remarks>
        public static T Acquire<T>()
            where T : class, new()
            => ObjectPool<T>.Acquire();

        /// <summary>
        /// Calls <see cref="ObjectPool{T}.Acquire"/> to get a spare item if there are any, or create a new one.
        /// </summary>
        /// <remarks>Remember to <see cref="Release{T}(T)"/> it when you are done.</remarks>
        public static void Acquire<T>(out T item)
            where T : class, new()
            => item = ObjectPool<T>.Acquire();

        /************************************************************************************************************************/

        /// <summary>
        /// Calls <see cref="ObjectPool{T}.Release"/> to add the `item` to the list of spares so it can be reused.
        /// </summary>
        public static void Release<T>(T item)
            where T : class, new()
            => ObjectPool<T>.Release(item);

        /// <summary>
        /// Calls <see cref="ObjectPool{T}.Release"/> to add the `item` to the list of spares so it can be reused.
        /// </summary>
        public static void Release<T>(ref T item) where T : class, new()
        {
            if (item != null)
            {
                ObjectPool<T>.Release(item);
                item = null;
            }
        }

        /************************************************************************************************************************/

        /// <summary>An error message for when something has been modified after being released to the pool.</summary>
        public const string
            NotClearError = " They must be cleared before being released to the pool and not modified after that.";

        /************************************************************************************************************************/

        /// <summary>
        /// Calls <see cref="ObjectPool{T}.Acquire"/> to get a spare <see cref="List{T}"/> if
        /// there are any or create a new one.
        /// </summary>
        /// <remarks>Remember to <see cref="Release{T}(List{T})"/> it when you are done.</remarks>
        public static List<T> AcquireList<T>()
        {
            var list = ObjectPool<List<T>>.Acquire();
            Debug.Assert(list.Count == 0, "A pooled list is not empty." + NotClearError);
            return list;
        }

        /// <summary>
        /// Calls <see cref="ObjectPool{T}.Release"/> to clear the `list` and mark it as a spare
        /// so it can be later returned by <see cref="AcquireList"/>.
        /// </summary>
        public static void Release<T>(List<T> list)
        {
            list.Clear();
            ObjectPool<List<T>>.Release(list);
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Calls <see cref="ObjectPool{T}.Acquire"/> to get a spare <see cref="HashSet{T}"/> if
        /// there are any or create a new one.
        /// </summary>
        /// <remarks>Remember to <see cref="Release{T}(HashSet{T})"/> it when you are done.</remarks>
        public static HashSet<T> AcquireSet<T>()
        {
            var set = ObjectPool<HashSet<T>>.Acquire();
            Debug.Assert(set.Count == 0, "A pooled set is not empty." + NotClearError);
            return set;
        }

        /// <summary>
        /// Calls <see cref="ObjectPool{T}.Release"/> to clear the `set` and mark it as a spare
        /// so it can be later returned by <see cref="AcquireSet"/>.
        /// </summary>
        public static void Release<T>(HashSet<T> set)
        {
            set.Clear();
            ObjectPool<HashSet<T>>.Release(set);
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Calls <see cref="ObjectPool{T}.Acquire"/> to get a spare <see cref="StringBuilder"/> if
        /// there are any or create a new one.
        /// </summary>
        /// <remarks>Remember to <see cref="Release(StringBuilder)"/> it when you are done.</remarks>
        public static StringBuilder AcquireStringBuilder()
        {
            var builder = ObjectPool<StringBuilder>.Acquire();
            Debug.Assert(builder.Length == 0, $"A pooled {nameof(StringBuilder)} is not empty." + NotClearError);
            return builder;
        }

        /// <summary>
        /// Sets <see cref="StringBuilder.Length"/> = 0 and <see cref="ObjectPool{T}.Release"/> to mark it as a spare
        /// so it can be later returned by <see cref="AcquireStringBuilder"/>.
        /// </summary>
        public static void Release(StringBuilder builder)
        {
            builder.Length = 0;
            ObjectPool<StringBuilder>.Release(builder);
        }

        /// <summary>
        /// Calls <see cref="StringBuilder.ToString()"/> and <see cref="Release(StringBuilder)"/>.
        /// </summary>
        public static string ReleaseToString(this StringBuilder builder)
        {
            var result = builder.ToString();
            Release(builder);
            return result;
        }

        /************************************************************************************************************************/

        private static class Cache<T>
        {
            public static readonly Dictionary<MethodInfo, KeyValuePair<Func<T>, T>>
                Results = new Dictionary<MethodInfo, KeyValuePair<Func<T>, T>>();
        }

        /// <summary>
        /// Creates an object using the provided delegate and caches it to return the same object when this method is
        /// called again for the same delegate.
        /// </summary>
        public static T GetCachedResult<T>(Func<T> function)
        {
            var method = function.Method;
            if (!Cache<T>.Results.TryGetValue(method, out var result))
            {

                result = new KeyValuePair<Func<T>, T>(function, function());
                Cache<T>.Results.Add(method, result);
            }
            else if (result.Key != function)
            {
                Debug.LogWarning(
                    $"{nameof(GetCachedResult)}<{typeof(T).Name}>" +
                    $" was previously called on {method.Name} with a different target." +
                    " This likely means that a new delegate is being passed into every call" +
                    " so it can't actually return the same cached object.");
            }

            return result.Value;
        }

        /************************************************************************************************************************/

        /// <summary>Convenience wrappers for <see cref="ObjectPool{T}.Disposable"/>.</summary>
        public static class Disposable
        {
            /************************************************************************************************************************/

            /// <summary>
            /// Calls <see cref="ObjectPool{T}.Disposable.Acquire"/> to get a spare <see cref="List{T}"/> if
            /// there are any or create a new one.
            /// </summary>
            public static IDisposable Acquire<T>(out T item)
                where T : class, new()
                => ObjectPool<T>.Disposable.Acquire(out item);

            /************************************************************************************************************************/

            /// <summary>
            /// Calls <see cref="ObjectPool{T}.Disposable.Acquire"/> to get a spare <see cref="List{T}"/> if
            /// there are any or create a new one.
            /// </summary>
            public static IDisposable AcquireList<T>(out List<T> list)
            {
                var disposable = ObjectPool<List<T>>.Disposable.Acquire(out list, onRelease: (l) => l.Clear());
                Debug.Assert(list.Count == 0, "A pooled list is not empty." + NotClearError);
                return disposable;
            }

            /************************************************************************************************************************/

            /// <summary>
            /// Calls <see cref="ObjectPool{T}.Disposable.Acquire"/> to get a spare <see cref="HashSet{T}"/> if
            /// there are any or create a new one.
            /// </summary>
            public static IDisposable AcquireSet<T>(out HashSet<T> set)
            {
                var disposable = ObjectPool<HashSet<T>>.Disposable.Acquire(out set, onRelease: (s) => s.Clear());
                Debug.Assert(set.Count == 0, "A pooled set is not empty." + NotClearError);
                return disposable;
            }

            /************************************************************************************************************************/
        }

        /************************************************************************************************************************/
    }

    /// <summary>A simple object pooling system.</summary>
    /// <remarks><typeparamref name="T"/> must not inherit from <see cref="Component"/> or <see cref="ScriptableObject"/>.</remarks>
    /// https://kybernetik.com.au/animancer/api/Animancer/ObjectPool_1
    /// 
    public static class ObjectPool<T> where T : class, new()
    {
        /************************************************************************************************************************/

        private static readonly List<T>
            Items = new List<T>();

        /************************************************************************************************************************/

        /// <summary>The number of spare items currently in the pool.</summary>
        public static int Count
        {
            get => Items.Count;
            set
            {
                var count = Items.Count;
                if (count < value)
                {
                    if (Items.Capacity < value)
                        Items.Capacity = Mathf.NextPowerOfTwo(value);

                    do
                    {
                        Items.Add(new T());
                        count++;
                    }
                    while (count < value);

                }
                else if (count > value)
                {
                    Items.RemoveRange(value, count - value);
                }
            }
        }

        /************************************************************************************************************************/

        /// <summary>
        /// If the <see cref="Count"/> is less than the specified value, this method increases it to that value by
        /// creating new objects.
        /// </summary>
        public static void SetMinCount(int count)
        {
            if (Count < count)
                Count = count;
        }

        /************************************************************************************************************************/

        /// <summary>The <see cref="List{T}.Capacity"/> of the internal list of spare items.</summary>
        public static int Capacity
        {
            get => Items.Capacity;
            set
            {
                if (Items.Count > value)
                    Items.RemoveRange(value, Items.Count - value);
                Items.Capacity = value;
            }
        }

        /************************************************************************************************************************/

        /// <summary>Returns a spare item if there are any, or creates a new one.</summary>
        /// <remarks>Remember to <see cref="Release(T)"/> it when you are done.</remarks>
        public static T Acquire()
        {
            var count = Items.Count;
            if (count == 0)
            {
                return new T();
            }
            else
            {
                count--;
                var item = Items[count];
                Items.RemoveAt(count);

                return item;
            }
        }

        /************************************************************************************************************************/

        /// <summary>Adds the `item` to the list of spares so it can be reused.</summary>
        public static void Release(T item)
        {
            Items.Add(item);

        }

        /************************************************************************************************************************/

        /// <summary>Returns a description of the state of this pool.</summary>
        public static string GetDetails()
        {
            return
                $"{typeof(T).Name}" +
                $" ({nameof(Count)} = {Items.Count}" +
                $", {nameof(Capacity)} = {Items.Capacity}" +
                ")";
        }

        /************************************************************************************************************************/

        /// <summary>
        /// An <see cref="IDisposable"/> system to allow pooled objects to be acquired and released within <c>using</c>
        /// statements instead of needing to manually release everything.
        /// </summary>
        public sealed class Disposable : IDisposable
        {
            /************************************************************************************************************************/

            private static readonly List<Disposable> LazyStack = new List<Disposable>();

            private static int _ActiveDisposables;

            private T _Item;
            private Action<T> _OnRelease;

            /************************************************************************************************************************/

            private Disposable() { }

            /// <summary>
            /// Calls <see cref="ObjectPool{T}.Acquire"/> to set the `item` and returns an <see cref="IDisposable"/>
            /// that will call <see cref="Release(T)"/> on the `item` when disposed.
            /// </summary>
            public static IDisposable Acquire(out T item, Action<T> onRelease = null)
            {
                Disposable disposable;

                if (LazyStack.Count <= _ActiveDisposables)
                {
                    LazyStack.Add(disposable = new Disposable());
                }
                else
                {
                    disposable = LazyStack[_ActiveDisposables];
                }

                _ActiveDisposables++;

                disposable._Item = item = ObjectPool<T>.Acquire();
                disposable._OnRelease = onRelease;
                return disposable;
            }

            /************************************************************************************************************************/

            void IDisposable.Dispose()
            {
                _OnRelease?.Invoke(_Item);
                Release(_Item);
                _ActiveDisposables--;
            }

            /************************************************************************************************************************/
        }

        /************************************************************************************************************************/
    }
}

