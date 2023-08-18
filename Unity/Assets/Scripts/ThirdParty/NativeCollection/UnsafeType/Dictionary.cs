using System;
using System.Collections;
using System.Collections.Generic;

namespace NativeCollection.UnsafeType
{
    public unsafe struct Dictionary<TKey, TValue> : IDictionary<TKey, TValue> where TKey : unmanaged, IEquatable<TKey> where TValue : unmanaged, IEquatable<TValue>
    {
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            throw new NotImplementedException();
        }
    
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    
        public void Add(KeyValuePair<TKey, TValue> item)
        {
            throw new NotImplementedException();
        }
       
        public void Clear()
        {
            throw new NotImplementedException();
        }
    
        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            throw new NotImplementedException();
        }
    
        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }
    
        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            throw new NotImplementedException();
        }
    
        public int Count { get; }
        public bool IsReadOnly { get; }
        public void Add(TKey key, TValue value)
        {
            throw new NotImplementedException();
        }
    
        public bool ContainsKey(TKey key)
        {
            throw new NotImplementedException();
        }
    
        public bool Remove(TKey key)
        {
            throw new NotImplementedException();
        }
    
        public bool TryGetValue(TKey key, out TValue value)
        {
            throw new NotImplementedException();
        }
    
        public TValue this[TKey key]
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }
    
        public ICollection<TKey> Keys { get; }
        public ICollection<TValue> Values { get; }
    }
}

