// Animancer // https://kybernetik.com.au/animancer // Copyright 2020 Kybernetik //

//#define LOG_STRING_CACHE

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Animancer.Editor
{
    /// <summary>
    /// A simple system for converting objects and storing the results so they can be reused to minimise the need for
    /// garbage collection, particularly for string construction.
    /// </summary>
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor/ConversionCache_2
    /// 
    public sealed class ConversionCache<TKey, TValue>
    {
        /************************************************************************************************************************/

        private sealed class CachedValue
        {
            public int lastFrameAccessed;
            public TValue value;
        }

        /************************************************************************************************************************/

        private readonly Dictionary<TKey, CachedValue>
            Cache = new Dictionary<TKey, CachedValue>();
        private readonly List<TKey>
            Keys = new List<TKey>();
        private readonly Func<TKey, TValue>
            ConvertToValue;

        private int _LastCleanupFrame;

        /************************************************************************************************************************/

        /// <summary>
        /// Creates a new <see cref="ConversionCache{TKey, TValue}"/> which uses the specified delegate to convert values.
        /// </summary>
        public ConversionCache(Func<TKey, TValue> convertToValue) => ConvertToValue = convertToValue;

        /************************************************************************************************************************/

        /// <summary>
        /// If a value has already been cached for the specified `key`, return it. Otherwise create a new one using
        /// the delegate provided in the constructor and cache it.
        /// <para></para>
        /// This method also periodically removes values that have not been used recently.
        /// </summary>
        public TValue Convert(TKey key)
        {
            CachedValue cached;

            // The next time a value is retrieved after at least 100 frames, clear out any old ones.
            var frame = Time.frameCount;
            if (_LastCleanupFrame + 100 < frame)
            {

                for (int i = Keys.Count - 1; i >= 0; i--)
                {
                    var checkKey = Keys[i];
                    if (!Cache.TryGetValue(checkKey, out cached) ||
                        cached.lastFrameAccessed <= _LastCleanupFrame)
                    {
                        Cache.Remove(checkKey);
                        Keys.RemoveAt(i);
                    }
                }

                _LastCleanupFrame = frame;

            }

            if (!Cache.TryGetValue(key, out cached))
            {
                Cache.Add(key, cached = new CachedValue { value = ConvertToValue(key) });
                Keys.Add(key);

            }

            cached.lastFrameAccessed = frame;

            return cached.value;
        }

        /************************************************************************************************************************/
    }
}

