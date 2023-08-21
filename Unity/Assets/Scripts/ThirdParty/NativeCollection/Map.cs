using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using NativeCollection.UnsafeType;

namespace NativeCollection
{
    public unsafe class Map<T, K> : IEnumerable<MapPair<T, K>>, INativeCollectionClass
        where T : unmanaged, IEquatable<T>, IComparable<T> where K : unmanaged, IEquatable<K>
    {
        private const int _defaultPoolSize = 50;
        private int _poolSize;
        private UnsafeType.Map<T, K>* _map;
    
        public Map(int maxPoolSize = _defaultPoolSize)
        {
            _poolSize = maxPoolSize;
            _map = UnsafeType.Map<T, K>.Create(_poolSize);
            IsDisposed = false;
        }
    
        public K this[T key]
        {
            get => (*_map)[key];
            set => (*_map)[key] = value;
        }
    
        public int Count => _map->Count;
    
        IEnumerator<MapPair<T, K>> IEnumerable<MapPair<T, K>>.GetEnumerator()
        {
            return GetEnumerator();
        }
    
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    
        public void Dispose()
        {
            if (IsDisposed) return;
            if (_map != null)
            {
                _map->Dispose();
                NativeMemoryHelper.Free(_map);
                NativeMemoryHelper.RemoveNativeMemoryByte(Unsafe.SizeOf<UnsafeType.Map<T, K>>());
                IsDisposed = true;
            }
        }
    
        public void ReInit()
        {
            if (IsDisposed)
            {
                _map = UnsafeType.Map<T, K>.Create(_poolSize);
                IsDisposed = false;
            }
        }
    
        public bool IsDisposed { get; private set; }
    
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(T key, K value)
        {
            _map->Add(key, value);
        }
    
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove(T key)
        {
            return _map->Remove(key);
        }
    
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            _map->Clear();
        }
    
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeType.SortedSet<MapPair<T, K>>.Enumerator GetEnumerator()
        {
            return _map->GetEnumerator();
        }
    
        ~Map()
        {
            Dispose();
        }
    }
}

