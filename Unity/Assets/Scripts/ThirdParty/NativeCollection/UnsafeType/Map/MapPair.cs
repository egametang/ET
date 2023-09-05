using System;
using System.Runtime.CompilerServices;

namespace NativeCollection.UnsafeType
{
    public unsafe struct MapPair<T, K> : IEquatable<MapPair<T, K>>, IComparable<MapPair<T, K>>
        where T : unmanaged, IEquatable<T>, IComparable<T> where K : unmanaged, IEquatable<K>
    {
        public T Key { get; private set; }
        public K Value => _value;

        internal K _value;

        public MapPair(T key,K value = default)
        {
            Key = key;
            _value = value;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(MapPair<T, K> other)
        {
            return Key.Equals(other.Key);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int CompareTo(MapPair<T, K> other)
        {
            return Key.CompareTo(other.Key);
        }
    
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
        {
            return Key.GetHashCode();
        }
    }
}

