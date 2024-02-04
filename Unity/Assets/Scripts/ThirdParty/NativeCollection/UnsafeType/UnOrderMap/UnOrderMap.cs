using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

namespace NativeCollection.UnsafeType
{
public unsafe struct UnOrderMap<T,K> : IEnumerable<MapPair<T, K>>
    where T : unmanaged, IEquatable<T>, IComparable<T> where K : unmanaged, IEquatable<K>
{
    /// <summary>Cutoff point for stackallocs. This corresponds to the number of ints.</summary>
    private const int StackAllocThreshold = 100;

    /// <summary>
    ///     When constructing a hashset from an existing collection, it may contain duplicates,
    ///     so this is used as the max acceptable excess ratio of capacity to count. Note that
    ///     this is only used on the ctor and not to automatically shrink if the hashset has, e.g,
    ///     a lot of adds followed by removes. Users must explicitly shrink by calling TrimExcess.
    ///     This is set to 3 because capacity is acceptable as 2x rounded up to nearest prime.
    /// </summary>
    private const int ShrinkThreshold = 3;

    private const int StartOfFreeList = -3;

    private UnOrderMap<T,K>* _self;
    private int* _buckets;
    private int _bucketLength;
    private Entry* _entries;
    private int _entryLength;
#if TARGET_64BIT
        private ulong _fastModMultiplier;
#endif
    private int _count;
    private int _freeList;
    private int _freeCount;
    private int _version;

    public static UnOrderMap<T,K>* Create(int capacity = 0)
    {
        UnOrderMap<T,K>* unOrderMap = (UnOrderMap<T,K>*)NativeMemoryHelper.Alloc((UIntPtr)Unsafe.SizeOf<UnOrderMap<T,K>>());
        unOrderMap->_buckets = null;
        unOrderMap->_entries = null;
        unOrderMap->_self = unOrderMap;
        unOrderMap->Initialize(capacity);
        return unOrderMap;
    }
    
    public K this[T key]
    {
        get
        {
            bool contains = TryGetValue(key, out var value);
            if (contains)
            {
                return value;
            }
            return default;
        }
        set
        {
            bool modified = TryInsert(key, value, InsertionBehavior.OverwriteExisting);
            Debug.Assert(modified);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Add(T key,K value) => AddRef(key,value);
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool AddRef(in T key, in K value) => TryInsert(key,value, InsertionBehavior.ThrowOnExisting);

    IEnumerator<MapPair<T, K>> IEnumerable<MapPair<T, K>>.GetEnumerator()
    {
        return GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Enumerator GetEnumerator() => new Enumerator(_self);

    #region ICollection<T> methods

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear()
    {
        int count = _count;
        if (count > 0)
        {
            Debug.Assert(_buckets != null, "_buckets should be non-null");
            Debug.Assert(_entries != null, "_entries should be non-null");
            Unsafe.InitBlockUnaligned(_buckets,0,(uint)(Unsafe.SizeOf<int>()*_bucketLength));
            Unsafe.InitBlockUnaligned(_entries,0,(uint)(Unsafe.SizeOf<int>()*count));
            _count = 0;
            _freeList = -1;
            _freeCount = 0;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool ContainsKey(T key)
    {
        return FindItemIndex(key) >= 0;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool ContainsKeyRef(in T key)
    {
        return FindItemIndex(key) >= 0;
    }

    #endregion

    
    public void CopyTo(T[] array, int arrayIndex)
    {
        throw new NotImplementedException();
    }
    
    public bool RemoveRef(in T key)
    {
        //if (_buckets == null) return false;
        var entries = _entries;
        Debug.Assert(entries != null, "entries should be non-null");

        uint collisionCount = 0;
        int last = -1;
        int hashCode = key.GetHashCode();

        ref int bucket = ref GetBucketRef(hashCode);
        int i = bucket - 1; // Value in buckets is 1-based

        while (i >= 0)
        {
            ref Entry entry = ref entries[i];

            if (entry.HashCode == hashCode && (key.Equals(entry.Key)))
            {
                if (last < 0)
                {
                    bucket = entry.Next + 1; // Value in buckets is 1-based
                }
                else
                {
                    entries[last].Next = entry.Next;
                }

                Debug.Assert((StartOfFreeList - _freeList) < 0, "shouldn't underflow because max hashtable length is MaxPrimeArrayLength = 0x7FEFFFFD(2146435069) _freelist underflow threshold 2147483646");
                entry.Next = StartOfFreeList - _freeList;

                _freeList = i;
                _freeCount++;
                return true;
            }

            last = i;
            i = entry.Next;

            collisionCount++;
            if (collisionCount > (uint)_entryLength)
            {
                // The chain of entries forms a loop; which means a concurrent update has happened.
                // Break out of the loop and throw, rather than looping forever.
                ThrowHelper.ConcurrentOperationsNotSupported();
            }
        }

        return false;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Remove(T item)
    {
        return RemoveRef(item);
    }

    public bool TryGetValue(in T key, out K actualValue)
    {
        int index = FindItemIndex(key);
        if (index>=0)
        {
            actualValue = _entries[index].Value;
            return true;
        }
        actualValue = default;
        return false;
    }

    public int Count => _count - _freeCount;

    public void Dispose()
    {
        NativeMemoryHelper.Free(_buckets);
        NativeMemoryHelper.RemoveNativeMemoryByte(Unsafe.SizeOf<int>()*_bucketLength);
        
        NativeMemoryHelper.Free(_entries);
        NativeMemoryHelper.RemoveNativeMemoryByte(Unsafe.SizeOf<Entry>()*_entryLength);
    }
    
    #region Helper methods
    
    /// <summary>
    /// Initializes buckets and slots arrays. Uses suggested capacity by finding next prime
    /// greater than or equal to capacity.
    /// </summary>
    private int Initialize(int capacity)
    {
        int size = HashHelpers.GetPrime(capacity);
        _buckets = (int*)NativeMemoryHelper.AllocZeroed((UIntPtr)(Unsafe.SizeOf<int>() * size));
        _bucketLength = size;
        _entries = (Entry*)NativeMemoryHelper.AllocZeroed((UIntPtr)(Unsafe.SizeOf<Entry>() * size));
        _entryLength = size;
        // Assign member variables after both arrays are allocated to guard against corruption from OOM if second fails.
        _freeList = -1;
        _freeCount = 0;
        _count = 0;
        _version = 0;
#if TARGET_64BIT
            _fastModMultiplier = HashHelpers.GetFastModMultiplier((uint)size);
#endif

        return size;
    }
    
    
    /// <summary>Adds the specified element to the set if it's not already contained.</summary>
    /// <param name="value">The element to add to the set.</param>
    /// <param name="location">The index into <see cref="_entries" /> of the element.</param>
    /// <returns>true if the element is added to the <see cref="HashSet{T}" /> object; false if the element is already present.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool TryInsert(in T key,in K value,InsertionBehavior insertionBehavior)
    {
        //Console.WriteLine($"AddIfNotPresent:{value}");
        //if (_buckets == null) Initialize(0);
        Debug.Assert(_buckets != null);

        Entry* entries = _entries;
        Debug.Assert(entries != null, "expected entries to be non-null");

        //var comparer = _comparer;
        int hashCode;

        uint collisionCount = 0;
        ref var bucket = ref Unsafe.NullRef<int>();
        
        hashCode =  key.GetHashCode();
        bucket = ref GetBucketRef(hashCode);
        
        var i = bucket - 1; // Value in _buckets is 1-based
        // Console.WriteLine($"i:{i}");
        while (i >= 0)
        {
            // Console.WriteLine($"i:{i}");
            ref Entry entry = ref _entries[i];
            // Console.WriteLine($"entry.HashCode:{entry.HashCode} hashCode:{hashCode} Equals:{comparer.Equals(entry.Value, value)}");
            if (entry.HashCode == hashCode && entry.Key.Equals(key))
            {
                if (insertionBehavior== InsertionBehavior.OverwriteExisting)
                {
                    entries[i].Value = value;
                    return true;
                }
                if (insertionBehavior == InsertionBehavior.ThrowOnExisting)
                {
                    ThrowHelper.ThrowAddingDuplicateWithKeyArgumentException();
                }

                return false;
            }

            i = entry.Next;

            collisionCount++;
            // Console.WriteLine($"collisionCount :{collisionCount} i:{i}");
            if (collisionCount > (uint)_entryLength)
                // The chain of entries forms a loop, which means a concurrent update has happened.
                ThrowHelper.ConcurrentOperationsNotSupported();
        }
        

        int index;
        if (_freeCount > 0)
        {
            index = _freeList;
            _freeCount--;
            Debug.Assert(StartOfFreeList - _entries[_freeList].Next >= -1,
                "shouldn't overflow because `next` cannot underflow");
            _freeList = StartOfFreeList - _entries[_freeList].Next;
        }
        else
        {
            var count = _count;
            if (count == _entryLength)
            {
                Resize();
                bucket = ref GetBucketRef(hashCode);
            }

            index = count;
            _count = count + 1;
        }

        {
            ref Entry entry = ref _entries[index];
            entry.HashCode = hashCode;
            entry.Next = bucket - 1; // Value in _buckets is 1-based
            entry.Key = key;
            entry.Value = value;
            bucket = index + 1;
            _version++;
        }
        
        return true;
    }
        
        /// <summary>Gets a reference to the specified hashcode's bucket, containing an index into <see cref="_entries"/>.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ref int GetBucketRef(int hashCode)
        {
            //var buckets = _buckets;
            
#if TARGET_64BIT
            return ref _buckets[HashHelpers.FastMod((uint)hashCode, (uint)_buckets.Length, _fastModMultiplier)];
#else
            int index = (int)((uint)hashCode %(uint)_bucketLength);
            return ref _buckets[index];
#endif
        }

    #endregion

    

            /// <summary>Ensures that this hash set can hold the specified number of elements without growing.</summary>
        public int EnsureCapacity(int capacity)
        {
            if (capacity < 0)
            {
                ThrowHelper.HashSetCapacityOutOfRange();
            }

            int currentCapacity = _entries == null ? 0 : _entryLength;
            if (currentCapacity >= capacity)
            {
                return currentCapacity;
            }

            if (_buckets == null)
            {
                return Initialize(capacity);
            }

            int newSize = HashHelpers.GetPrime(capacity);
            Resize(newSize, forceNewHashCodes: false);
            return newSize;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Resize() => Resize(HashHelpers.ExpandPrime(_count), forceNewHashCodes: false);

        private void Resize(int newSize, bool forceNewHashCodes)
        {
            // Console.WriteLine($"before resize:{*self}");
            // Value types never rehash
            Debug.Assert(!forceNewHashCodes || !typeof(T).IsValueType);
            Debug.Assert(_entries != null, "_entries should be non-null");
            Debug.Assert(newSize >= _entryLength);
            // Console.WriteLine($"Resize newSize:{newSize} byteSize:{Unsafe.SizeOf<Entry>() * newSize}");
            var newEntries = (Entry*)NativeMemoryHelper.AllocZeroed((UIntPtr)(Unsafe.SizeOf<Entry>() * newSize));
            Unsafe.CopyBlockUnaligned(newEntries,_entries,(uint)(Unsafe.SizeOf<Entry>()*_entryLength));
            int count = _count;
            // Assign member variables after both arrays allocated to guard against corruption from OOM if second fails
            var newBucket = (int*)NativeMemoryHelper.AllocZeroed((UIntPtr)(Unsafe.SizeOf<int>() * newSize));
            NativeMemoryHelper.Free(_buckets);
            NativeMemoryHelper.RemoveNativeMemoryByte(Unsafe.SizeOf<int>()*_bucketLength);
            _buckets = newBucket;
            _bucketLength = newSize;
#if TARGET_64BIT
            _fastModMultiplier = HashHelpers.GetFastModMultiplier((uint)newSize);
#endif
            for (int i = 0; i < count; i++)
            {
                ref Entry entry = ref newEntries[i];
                if (entry.Next >= -1)
                {
                    ref int bucket = ref GetBucketRef(entry.HashCode);
                    entry.Next = bucket - 1; // Value in _buckets is 1-based
                    // Console.WriteLine($"entry.Next:{entry.Next} bucket:{bucket}");
                    bucket = i + 1;
                }
            }
            NativeMemoryHelper.Free(_entries);
            NativeMemoryHelper.RemoveNativeMemoryByte(Unsafe.SizeOf<Entry>()*_entryLength);
            _entries = newEntries;
            _entryLength = newSize;
            
            //Console.WriteLine($"after resize:{*self} totalSize:{_entryLength}");
        }
        
        /// <summary>Gets the index of the item in <see cref="_entries"/>, or -1 if it's not in the set.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int FindItemIndex(in T key)
        {
            //if (_buckets == null) return -1;
            var entries = _entries;
            Debug.Assert(entries != null, "Expected _entries to be initialized");

            uint collisionCount = 0;
            //IEqualityComparer<T>? comparer = _comparer;
            
            int hashCode = key.GetHashCode();
            int i = GetBucketRef(hashCode) - 1; // Value in _buckets is 1-based
            while (i >= 0)
            {
                ref Entry entry = ref entries[i];
                if (entry.HashCode == hashCode && key.Equals(entry.Key))
                {
                    return i;
                }
                i = entry.Next;

                collisionCount++;
                if (collisionCount > (uint)_entryLength)
                {
                    // The chain of entries forms a loop, which means a concurrent update has happened.
                    ThrowHelper.ConcurrentOperationsNotSupported();
                }
            }

            return -1;
        }

    
    private struct Entry : IEquatable<Entry>
    {
        public int HashCode;

        /// <summary>
        /// 0-based index of next entry in chain: -1 means end of chain
        /// also encodes whether this entry _itself_ is part of the free list by changing sign and subtracting 3,
        /// so -2 means end of free list, -3 means index 0 but on free list, -4 means index 1 but on free list, etc.
        /// </summary>
        public int Next;
        public T Key;
        public K Value;
        public bool Equals(Entry other)
        {
            return HashCode == other.HashCode && Next == other.Next && Key.Equals(other.Key);
        }
    }

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();
        foreach (var value in *_self)
        {
            sb.Append($"{value} ");
        }

        sb.Append("\n");
        return sb.ToString();
    }
    
    internal enum InsertionBehavior : byte
    {
        None,
        OverwriteExisting,
        ThrowOnExisting,
    }

    public struct Enumerator : IEnumerator<MapPair<T, K>>
    {
        private readonly UnOrderMap<T,K>* _unOrderMap;
        private readonly int _version;
        private int _index;
        private MapPair<T, K> _current;

        internal Enumerator(UnOrderMap<T,K>* unOrderMap)
        {
            _unOrderMap = unOrderMap;
            _version = unOrderMap->_version;
            _index = 0;
            _current = default!;
        }

        public bool MoveNext()
        {
            if (_version != _unOrderMap->_version)
                ThrowHelper.HashSetEnumFailedVersion();
            
            // Use unsigned comparison since we set index to dictionary.count+1 when the enumeration ends.
            // dictionary.count+1 could be negative if dictionary.count is int.MaxValue
            while ((uint)_index < (uint)_unOrderMap->_count)
            {
                ref Entry entry = ref _unOrderMap->_entries[_index++];
                if (entry.Next >= -1)
                {
                    _current = new MapPair<T, K>(entry.Key,entry.Value);
                    return true;
                }
            }

            _index = _unOrderMap->_count + 1;
            _current = default!;
            return false;
        }

        public MapPair<T, K> Current => _current;

        public void Dispose()
        {
        }
        
        object IEnumerator.Current => Current;
        
        public void Reset()
        {
            if (_version != _unOrderMap->_version)
                ThrowHelper.HashSetEnumFailedVersion();

            _index = 0;
            _current = default!;
        }
    }
}
}

