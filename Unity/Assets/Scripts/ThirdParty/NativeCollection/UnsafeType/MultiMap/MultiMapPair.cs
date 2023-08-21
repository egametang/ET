using System;
using System.Runtime.CompilerServices;

namespace NativeCollection.UnsafeType
{
    public unsafe struct MultiMapPair<T, K> : IEquatable<MultiMapPair<T, K>>, IComparable<MultiMapPair<T, K>>
        where T : unmanaged, IEquatable<T>, IComparable<T> where K : unmanaged, IEquatable<K>
    {
        private UnsafeType.List<K>* _value;
        
        public T Key { get; private set; }
    
        public ref UnsafeType.List<K> Value => ref Unsafe.AsRef<UnsafeType.List<K>>(_value);
    
        public MultiMapPair(T key)
        {
            Key = key;
            _value = null;
        }
    
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MultiMapPair<T, K> Create(in T key, NativePool<List<K>>* pool)
        {
            var pair = new MultiMapPair<T, K>(key);
            var list = pool->Alloc();
            if (list==null)
            {
                list = List<K>.Create();
            }
            pair._value = list;
            return pair;
        }
    
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(MultiMapPair<T, K> other)
        {
            return Key.Equals(other.Key);
        }
    
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int CompareTo(MultiMapPair<T, K> other)
        {
            return Key.CompareTo(other.Key);
        }
    
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
        {
            return Key.GetHashCode();
        }
    
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose(NativePool<List<K>>* pool)
        {
            if (_value!=null)
            {
                pool->Return(_value);
            }
        }
    }
}

