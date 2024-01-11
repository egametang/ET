using System;
using System.Collections.Generic;
using UnityEngine;

namespace YIUIFramework
{
    /// <summary>
    /// This is a serializable dictionary.
    /// </summary>
    [Serializable]
    public sealed class SerializableDictionary<TKey, TValue> :
        Dictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        [SerializeField]
        private KeyValuePair[] data;

        /// <inheritdoc/>
        public void OnBeforeSerialize()
        {
            this.data = new KeyValuePair[this.Count];
            int index = 0;
            foreach (var kv in this)
            {
                this.data[index++] = new KeyValuePair(kv.Key, kv.Value);
            }
        }

        /// <inheritdoc/>
        public void OnAfterDeserialize()
        {
            this.Clear();
            foreach (var kv in this.data)
            {
                this.Add(kv.Key, kv.Value);
            }

            this.data = null;
        }

        [Serializable]
        private struct KeyValuePair
        {
            [SerializeField]
            public TKey Key;

            [SerializeField]
            public TValue Value;

            public KeyValuePair(TKey k, TValue v)
            {
                this.Key   = k;
                this.Value = v;
            }
        }
    }
}