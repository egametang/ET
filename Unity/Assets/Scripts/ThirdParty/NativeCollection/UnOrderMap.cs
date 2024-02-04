using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using NativeCollection.UnsafeType;

namespace NativeCollection
{
    public unsafe class UnOrderMap<T, K> : IEnumerable<MapPair<T, K>>, INativeCollectionClass
        where T : unmanaged, IEquatable<T>, IComparable<T> where K : unmanaged, IEquatable<K>
    {
private int _capacity;
        private UnsafeType.UnOrderMap<T, K>* _unOrderMap;

        public UnOrderMap(int initCapacity = 0)
        {
            _capacity = initCapacity;
            _unOrderMap = UnsafeType.UnOrderMap<T, K>.Create(_capacity);
            IsDisposed = false;
        }

        public K this[T key]
        {
            get => (*_unOrderMap)[key];
            set => (*_unOrderMap)[key] = value;
        }
        
        public int Count => _unOrderMap->Count;
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(in T key, K value)
        {
            _unOrderMap->Add(key, value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove(in T key)
        {
            return _unOrderMap->Remove(key);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            _unOrderMap->Clear();
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ContainsKey(in T key)
        {
            return _unOrderMap->ContainsKey(key);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValue(in T key, out K value)
        {
            bool contains =  _unOrderMap->TryGetValue(key, out var actualValue);
            if (contains)
            {
                value = actualValue;
                return true;
            }
            value = default;
            return false;
        }

        
        IEnumerator<MapPair<T, K>> IEnumerable<MapPair<T, K>>.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeType.UnOrderMap<T,K>.Enumerator GetEnumerator()
        {
            return _unOrderMap->GetEnumerator();
        }

        public void Dispose()
        {
            if (IsDisposed) return;
            if (_unOrderMap != null)
            {
                _unOrderMap->Dispose();
                NativeMemoryHelper.Free(_unOrderMap);
                NativeMemoryHelper.RemoveNativeMemoryByte(Unsafe.SizeOf<UnsafeType.UnOrderMap<T, K>>());
                IsDisposed = true;
            }
        }

        public void ReInit()
        {
            if (IsDisposed)
            {
                _unOrderMap = UnsafeType.UnOrderMap<T, K>.Create(_capacity);
                IsDisposed = false;
            }
        }

        public bool IsDisposed { get; private set; }
        
        ~UnOrderMap()
        {
            Dispose();
        }
    }
}

