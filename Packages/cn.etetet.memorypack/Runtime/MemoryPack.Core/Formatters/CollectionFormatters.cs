using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

#nullable enable
using MemoryPack.Formatters;
using MemoryPack.Internal;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// Clear support collection formatters
// List, Stack, Queue, LinkedList, HashSet, PriorityQueue,
// ObservableCollection, Collection
// ConcurrentQueue, ConcurrentStack, ConcurrentBag
// Dictionary, SortedDictionary, SortedList, ConcurrentDictionary

// Not supported clear
// ReadOnlyCollection, ReadOnlyObservableCollection, BlockingCollection

namespace MemoryPack
{
    public static partial class MemoryPackFormatterProvider
    {
        static readonly Dictionary<Type, Type> CollectionFormatters = new Dictionary<Type, Type>(18)
        {
            { typeof(List<>), typeof(ListFormatter<>) },
            { typeof(Stack<>), typeof(StackFormatter<>) },
            { typeof(Queue<>), typeof(QueueFormatter<>) },
            { typeof(LinkedList<>), typeof(LinkedListFormatter<>) },
            { typeof(HashSet<>), typeof(HashSetFormatter<>) },
            { typeof(SortedSet<>), typeof(SortedSetFormatter<>) },
#if NET7_0_OR_GREATER
            { typeof(PriorityQueue<,>), typeof(PriorityQueueFormatter<,>) },
#endif
            { typeof(ObservableCollection<>), typeof(ObservableCollectionFormatter<>) },
            { typeof(Collection<>), typeof(CollectionFormatter<>) },
            { typeof(ConcurrentQueue<>), typeof(ConcurrentQueueFormatter<>) },
            { typeof(ConcurrentStack<>), typeof(ConcurrentStackFormatter<>) },
            { typeof(ConcurrentBag<>), typeof(ConcurrentBagFormatter<>) },
            { typeof(Dictionary<,>), typeof(DictionaryFormatter<,>) },
            { typeof(SortedDictionary<,>), typeof(SortedDictionaryFormatter<,>) },
            { typeof(SortedList<,>), typeof(SortedListFormatter<,>) },
            { typeof(ConcurrentDictionary<,>), typeof(ConcurrentDictionaryFormatter<,>) },
            { typeof(ReadOnlyCollection<>), typeof(ReadOnlyCollectionFormatter<>) },
            { typeof(ReadOnlyObservableCollection<>), typeof(ReadOnlyObservableCollectionFormatter<>) },
            { typeof(BlockingCollection<>), typeof(BlockingCollectionFormatter<>) },
        };
    }
}

namespace MemoryPack.Formatters
{
    [Preserve]
    public static class ListFormatter
    {
        [Preserve]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SerializePackable<T>(ref MemoryPackWriter writer, ref List<T?>? value)
            where T : IMemoryPackable<T>
#if NET7_0_OR_GREATER
            
#else
            
#endif
        {
            if (value == null)
            {
                writer.WriteNullCollectionHeader();
                return;
            }

#if NET7_0_OR_GREATER
            writer.WritePackableSpan(CollectionsMarshal.AsSpan(value));
#else
            var formatter = writer.GetFormatter<T?>();
            writer.WriteCollectionHeader(value.Count);
            foreach (var item in value)
            {
                var v = item;
                formatter.Serialize(ref writer, ref v);
            }
#endif
        }

        [Preserve]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static List<T?>? DeserializePackable<T>(ref MemoryPackReader reader)
            where T : IMemoryPackable<T>
        {
            List<T?>? value = default;
            DeserializePackable<T>(ref reader, ref value);
            return value;
        }

        [Preserve]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DeserializePackable<T>(ref MemoryPackReader reader, ref List<T?>? value)
            where T : IMemoryPackable<T>
        {
            if (!reader.TryReadCollectionHeader(out var length))
            {
                value = null;
                return;
            }

            if (value == null)
            {
                value = new List<T?>(length);
            }
#if NET7_0_OR_GREATER
            else if (value.Count == length)
            {
                value.Clear();
            }

            var span = CollectionsMarshalEx.CreateSpan(value, length);
            reader.ReadPackableSpanWithoutReadLengthHeader(length, ref span);
#else
            else
            {
                value.Clear();
            }
            var formatter = reader.GetFormatter<T?>();
            for (var i = 0; i < length; i++)
            {
                T? v = default;
                formatter.Deserialize(ref reader, ref v);
                value.Add(v);
            }
#endif
        }
    }

    [Preserve]
    public sealed class ListFormatter<T> : MemoryPackFormatter<List<T?>>
    {
        [Preserve]
        public override void Serialize(ref MemoryPackWriter writer, ref List<T?>? value)
        {
            if (value == null)
            {
                writer.WriteNullCollectionHeader();
                return;
            }
#if NET7_0_OR_GREATER
            writer.WriteSpan(CollectionsMarshal.AsSpan(value));
#else
            var formatter = writer.GetFormatter<T?>();
            writer.WriteCollectionHeader(value.Count);
            foreach (var item in value)
            {
                var v = item;
                formatter.Serialize(ref writer, ref v);
            }
#endif
        }

        [Preserve]
        public override void Deserialize(ref MemoryPackReader reader, ref List<T?>? value)
        {
            if (!reader.TryReadCollectionHeader(out var length))
            {
                value = null;
                return;
            }

            if (value == null)
            {
                value = new List<T?>(length);
            }
#if NET7_0_OR_GREATER
            else if (value.Count == length)
            {
                value.Clear();
            }

            var span = CollectionsMarshalEx.CreateSpan(value, length);
            reader.ReadSpanWithoutReadLengthHeader(length, ref span);
#else
            else
            {
                value.Clear();
            }
            var formatter = reader.GetFormatter<T?>();
            for (var i = 0; i < length; i++)
            {
                T? v = default;
                formatter.Deserialize(ref reader, ref v);
                value.Add(v);
            }
#endif
        }
    }

    [Preserve]
    public sealed class StackFormatter<T> : MemoryPackFormatter<Stack<T?>>
    {
        [Preserve]
        public override void Serialize(ref MemoryPackWriter writer, ref Stack<T?>? value)
        {
            if (value == null)
            {
                writer.WriteNullCollectionHeader();
                return;
            }

#if NET7_0_OR_GREATER
            writer.WriteSpan(CollectionsMarshalEx.AsSpan(value));
#else
            var formatter = writer.GetFormatter<T?>();
            writer.WriteCollectionHeader(value.Count);
            foreach (var item in value.Reverse()) // serialize reverse order
            {
                var v = item;
                formatter.Serialize(ref writer, ref v);
            }
#endif
        }

        [Preserve]
        public override void Deserialize(ref MemoryPackReader reader, ref Stack<T?>? value)
        {
            if (!reader.TryReadCollectionHeader(out var length))
            {
                value = null;
                return;
            }

            if (value == null)
            {
                value = new Stack<T?>(length);
            }
#if NET7_0_OR_GREATER
            else if (value.Count != length)
            {
                value.Clear();
            }

            var span = CollectionsMarshalEx.CreateSpan(value, length);
            reader.ReadSpanWithoutReadLengthHeader(length, ref span);
#else
            else
            {
                value.Clear();
            }
            var formatter = reader.GetFormatter<T?>();
            for (int i = 0; i < length; i++)
            {
                T? v = default;
                formatter.Deserialize(ref reader, ref v);
                value.Push(v);
            }
#endif
        }
    }

    [Preserve]
    public sealed class QueueFormatter<T> : MemoryPackFormatter<Queue<T?>>
    {
        // Queue is circular buffer, can't optimize like List, Stack.

        [Preserve]
        public override void Serialize(ref MemoryPackWriter writer, ref Queue<T?>? value)
        {
            if (value == null)
            {
                writer.WriteNullCollectionHeader();
                return;
            }

            var formatter = writer.GetFormatter<T?>();
            writer.WriteCollectionHeader(value.Count);
            foreach (var item in value)
            {
                var v = item;
                formatter.Serialize(ref writer, ref v);
            }
        }

        [Preserve]
        public override void Deserialize(ref MemoryPackReader reader, ref Queue<T?>? value)
        {
            if (!reader.TryReadCollectionHeader(out var length))
            {
                value = null;
                return;
            }

            if (value == null)
            {
                value = new Queue<T?>(length);
            }
            else
            {
                value.Clear();
#if NET7_0_OR_GREATER
                value.EnsureCapacity(length);
#endif
            }

            var formatter = reader.GetFormatter<T?>();
            for (int i = 0; i < length; i++)
            {
                T? v = default;
                formatter.Deserialize(ref reader, ref v);
                value.Enqueue(v);
            }
        }
    }

    [Preserve]
    public sealed class LinkedListFormatter<T> : MemoryPackFormatter<LinkedList<T?>>
    {
        [Preserve]
        public override void Serialize(ref MemoryPackWriter writer, ref LinkedList<T?>? value)
        {
            if (value == null)
            {
                writer.WriteNullCollectionHeader();
                return;
            }

            var formatter = writer.GetFormatter<T?>();
            writer.WriteCollectionHeader(value.Count);
            foreach (var item in value)
            {
                var v = item;
                formatter.Serialize(ref writer, ref v);
            }
        }

        [Preserve]
        public override void Deserialize(ref MemoryPackReader reader, ref LinkedList<T?>? value)
        {
            if (!reader.TryReadCollectionHeader(out var length))
            {
                value = null;
                return;
            }

            if (value == null)
            {
                value = new LinkedList<T?>();
            }
            else
            {
                value.Clear();
            }

            var formatter = reader.GetFormatter<T?>();
            for (int i = 0; i < length; i++)
            {
                T? v = default;
                formatter.Deserialize(ref reader, ref v);
                value.AddLast(v);
            }
        }
    }

    [Preserve]
    public sealed class HashSetFormatter<T> : MemoryPackFormatter<HashSet<T?>>
    {
        readonly IEqualityComparer<T?>? equalityComparer;

        public HashSetFormatter()
            : this(null)
        {
        }

        public HashSetFormatter(IEqualityComparer<T?>? equalityComparer)
        {
            this.equalityComparer = equalityComparer;
        }

        [Preserve]
        public override void Serialize(ref MemoryPackWriter writer, ref HashSet<T?>? value)
        {
            if (value == null)
            {
                writer.WriteNullCollectionHeader();
                return;
            }

            var formatter = writer.GetFormatter<T?>();
            writer.WriteCollectionHeader(value.Count);
            foreach (var item in value)
            {
                var v = item;
                formatter.Serialize(ref writer, ref v);
            }
        }

        [Preserve]
        public override void Deserialize(ref MemoryPackReader reader, ref HashSet<T?>? value)
        {
            if (!reader.TryReadCollectionHeader(out var length))
            {
                value = null;
                return;
            }

            if (value == null)
            {
                value = new HashSet<T?>(length, equalityComparer);
            }
            else
            {
                value.Clear();
            }

            var formatter = reader.GetFormatter<T?>();
            for (int i = 0; i < length; i++)
            {
                T? v = default;
                formatter.Deserialize(ref reader, ref v);
                value.Add(v);
            }
        }
    }

    [Preserve]
    public sealed class SortedSetFormatter<T> : MemoryPackFormatter<SortedSet<T?>>
    {
        readonly IComparer<T?>? comparer;

        public SortedSetFormatter()
            : this(null)
        {
        }

        public SortedSetFormatter(IComparer<T?>? comparer)
        {
            this.comparer = comparer;
        }

        [Preserve]
        public override void Serialize(ref MemoryPackWriter writer, ref SortedSet<T?>? value)
        {
            if (value == null)
            {
                writer.WriteNullCollectionHeader();
                return;
            }

            var formatter = writer.GetFormatter<T?>();
            writer.WriteCollectionHeader(value.Count);
            foreach (var item in value)
            {
                var v = item;
                formatter.Serialize(ref writer, ref v);
            }
        }

        [Preserve]
        public override void Deserialize(ref MemoryPackReader reader, ref SortedSet<T?>? value)
        {
            if (!reader.TryReadCollectionHeader(out var length))
            {
                value = null;
                return;
            }

            if (value == null)
            {
                value = new SortedSet<T?>(comparer);
            }
            else
            {
                value.Clear();
            }

            var formatter = reader.GetFormatter<T?>();
            for (int i = 0; i < length; i++)
            {
                T? v = default;
                formatter.Deserialize(ref reader, ref v);
                value.Add(v);
            }
        }
    }

#if NET7_0_OR_GREATER

    [Preserve]
    public sealed class PriorityQueueFormatter<TElement, TPriority> : MemoryPackFormatter<PriorityQueue<TElement?, TPriority?>>
    {
        static PriorityQueueFormatter()
        {
            if (!MemoryPackFormatterProvider.IsRegistered<(TElement?, TPriority?)>())
            {
                MemoryPackFormatterProvider.Register(new ValueTupleFormatter<TElement?, TPriority?>());
            }
        }

        [Preserve]
        public override void Serialize(ref MemoryPackWriter writer, ref PriorityQueue<TElement?, TPriority?>? value)
        {
            if (value == null)
            {
                writer.WriteNullCollectionHeader();
                return;
            }

            var formatter = writer.GetFormatter<(TElement?, TPriority?)>();

            writer.WriteCollectionHeader(value.Count);
            foreach (var item in value.UnorderedItems)
            {
                var v = item;
                formatter.Serialize(ref writer, ref v);
            }
        }

        [Preserve]
        public override void Deserialize(ref MemoryPackReader reader, ref PriorityQueue<TElement?, TPriority?>? value)
        {
            if (!reader.TryReadCollectionHeader(out var length))
            {
                value = null;
                return;
            }

            if (value == null)
            {
                value = new PriorityQueue<TElement?, TPriority?>(length);
            }
            else
            {
                value.Clear();
            }

            var formatter = reader.GetFormatter<(TElement?, TPriority?)>();
            for (int i = 0; i < length; i++)
            {
                (TElement?, TPriority?) v = default;
                formatter.Deserialize(ref reader, ref v);
                value.Enqueue(v.Item1, v.Item2);
            }
        }
    }

#endif

    [Preserve]
    public sealed class CollectionFormatter<T> : MemoryPackFormatter<Collection<T?>>
    {
        [Preserve]
        public override void Serialize(ref MemoryPackWriter writer, ref Collection<T?>? value)
        {
            if (value == null)
            {
                writer.WriteNullCollectionHeader();
                return;
            }

            var formatter = writer.GetFormatter<T?>();
            writer.WriteCollectionHeader(value.Count);
            foreach (var item in value)
            {
                var v = item;
                formatter.Serialize(ref writer, ref v);
            }
        }

        [Preserve]
        public override void Deserialize(ref MemoryPackReader reader, ref Collection<T?>? value)
        {
            if (!reader.TryReadCollectionHeader(out var length))
            {
                value = null;
                return;
            }

            if (value == null)
            {
                value = new Collection<T?>();
            }
            else
            {
                value.Clear();
            }

            var formatter = reader.GetFormatter<T?>();
            for (int i = 0; i < length; i++)
            {
                T? v = default;
                formatter.Deserialize(ref reader, ref v);
                value.Add(v);
            }
        }
    }

    [Preserve]
    public sealed class ObservableCollectionFormatter<T> : MemoryPackFormatter<ObservableCollection<T?>>
    {
        [Preserve]
        public override void Serialize(ref MemoryPackWriter writer, ref ObservableCollection<T?>? value)
        {
            if (value == null)
            {
                writer.WriteNullCollectionHeader();
                return;
            }

            var formatter = writer.GetFormatter<T?>();
            writer.WriteCollectionHeader(value.Count);
            foreach (var item in value)
            {
                var v = item;
                formatter.Serialize(ref writer, ref v);
            }
        }

        [Preserve]
        public override void Deserialize(ref MemoryPackReader reader, ref ObservableCollection<T?>? value)
        {
            if (!reader.TryReadCollectionHeader(out var length))
            {
                value = null;
                return;
            }

            if (value == null)
            {
                value = new ObservableCollection<T?>();
            }
            else
            {
                value.Clear();
            }

            var formatter = reader.GetFormatter<T?>();
            for (int i = 0; i < length; i++)
            {
                T? v = default;
                formatter.Deserialize(ref reader, ref v);
                value.Add(v);
            }
        }
    }

    [Preserve]
    public sealed class ConcurrentQueueFormatter<T> : MemoryPackFormatter<ConcurrentQueue<T?>>
    {
        [Preserve]
        public override void Serialize(ref MemoryPackWriter writer, ref ConcurrentQueue<T?>? value)
        {
            if (value == null)
            {
                writer.WriteNullCollectionHeader();
                return;
            }

            // note: serializing ConcurretnCollection(Queue/Stack/Bag/Dictionary) is not thread-safe.
            // operate Add/Remove in iterating in other thread, not guranteed correct result

            var formatter = writer.GetFormatter<T?>();
            var count = value.Count;
            writer.WriteCollectionHeader(count);
            var i = 0;
            foreach (var item in value)
            {
                i++;
                var v = item;
                formatter.Serialize(ref writer, ref v);
            }

            if (i != count) MemoryPackSerializationException.ThrowInvalidConcurrrentCollectionOperation();
        }

        [Preserve]
        public override void Deserialize(ref MemoryPackReader reader, ref ConcurrentQueue<T?>? value)
        {
            if (!reader.TryReadCollectionHeader(out var length))
            {
                value = null;
                return;
            }

            if (value == null)
            {
                value = new ConcurrentQueue<T?>();
            }
            else
            {
                value.Clear();
            }

            var formatter = reader.GetFormatter<T?>();
            for (int i = 0; i < length; i++)
            {
                T? v = default;
                formatter.Deserialize(ref reader, ref v);
                value.Enqueue(v);
            }
        }
    }

    [Preserve]
    public sealed class ConcurrentStackFormatter<T> : MemoryPackFormatter<ConcurrentStack<T?>>
    {
        [Preserve]
        public override void Serialize(ref MemoryPackWriter writer, ref ConcurrentStack<T?>? value)
        {
            if (value == null)
            {
                writer.WriteNullCollectionHeader();
                return;
            }

            // reverse order in serialize
            var count = value.Count;
            T?[] rentArray = ArrayPool<T?>.Shared.Rent(count);
            try
            {
                var i = 0;
                foreach (var item in value)
                {
                    rentArray[i++] = item;
                }
                if (i != count) MemoryPackSerializationException.ThrowInvalidConcurrrentCollectionOperation();

                var formatter = writer.GetFormatter<T?>();
                writer.WriteCollectionHeader(count);
                for (i = i - 1; i >= 0; i--)
                {
                    formatter.Serialize(ref writer, ref rentArray[i]);
                }
            }
            finally
            {
                ArrayPool<T?>.Shared.Return(rentArray, clearArray: RuntimeHelpers.IsReferenceOrContainsReferences<T>());
            }
        }

        [Preserve]
        public override void Deserialize(ref MemoryPackReader reader, ref ConcurrentStack<T?>? value)
        {
            if (!reader.TryReadCollectionHeader(out var length))
            {
                value = null;
                return;
            }

            if (value == null)
            {
                value = new ConcurrentStack<T?>();
            }
            else
            {
                value.Clear();
            }

            var formatter = reader.GetFormatter<T?>();
            for (int i = 0; i < length; i++)
            {
                T? v = default;
                formatter.Deserialize(ref reader, ref v);
                value.Push(v);
            }
        }
    }

    [Preserve]
    public sealed class ConcurrentBagFormatter<T> : MemoryPackFormatter<ConcurrentBag<T?>>
    {
        [Preserve]
        public override void Serialize(ref MemoryPackWriter writer, ref ConcurrentBag<T?>? value)
        {
            if (value == null)
            {
                writer.WriteNullCollectionHeader();
                return;
            }

            var formatter = writer.GetFormatter<T?>();
            var count = value.Count;
            writer.WriteCollectionHeader(count);
            var i = 0;
            foreach (var item in value)
            {
                i++;
                var v = item;
                formatter.Serialize(ref writer, ref v);
            }

            if (i != count) MemoryPackSerializationException.ThrowInvalidConcurrrentCollectionOperation();
        }

        [Preserve]
        public override void Deserialize(ref MemoryPackReader reader, ref ConcurrentBag<T?>? value)
        {
            if (!reader.TryReadCollectionHeader(out var length))
            {
                value = null;
                return;
            }

            if (value == null)
            {
                value = new ConcurrentBag<T?>();
            }
            else
            {
                value.Clear();
            }

            var formatter = reader.GetFormatter<T?>();
            for (int i = 0; i < length; i++)
            {
                T? v = default;
                formatter.Deserialize(ref reader, ref v);
                value.Add(v);
            }
        }
    }

    [Preserve]
    public sealed class DictionaryFormatter<TKey, TValue> : MemoryPackFormatter<Dictionary<TKey, TValue?>>
        where TKey : notnull
    {
        readonly IEqualityComparer<TKey>? equalityComparer;

        public DictionaryFormatter()
            : this(null)
        {

        }

        public DictionaryFormatter(IEqualityComparer<TKey>? equalityComparer)
        {
            this.equalityComparer = equalityComparer;
        }

        [Preserve]
        public override void Serialize(ref MemoryPackWriter writer, ref Dictionary<TKey, TValue?>? value)
        {
            if (value == null)
            {
                writer.WriteNullCollectionHeader();
                return;
            }

            var keyFormatter = writer.GetFormatter<TKey>();
            var valueFormatter = writer.GetFormatter<TValue>();

            writer.WriteCollectionHeader(value.Count);
            foreach (var item in value)
            {
                KeyValuePairFormatter.Serialize(keyFormatter, valueFormatter, ref writer, item!);
            }
        }

        [Preserve]
        public override void Deserialize(ref MemoryPackReader reader, ref Dictionary<TKey, TValue?>? value)
        {
            if (!reader.TryReadCollectionHeader(out var length))
            {
                value = null;
                return;
            }

            if (value == null)
            {
                value = new Dictionary<TKey, TValue?>(length, equalityComparer);
            }
            else
            {
                value.Clear();
            }

            var keyFormatter = reader.GetFormatter<TKey>();
            var valueFormatter = reader.GetFormatter<TValue>();
            for (int i = 0; i < length; i++)
            {
                KeyValuePairFormatter.Deserialize(keyFormatter, valueFormatter, ref reader, out var k, out var v);
                value.Add(k!, v);
            }
        }
    }

    [Preserve]
    public sealed class SortedDictionaryFormatter<TKey, TValue> : MemoryPackFormatter<SortedDictionary<TKey, TValue?>>
        where TKey : notnull
    {
        readonly IComparer<TKey>? comparer;

        public SortedDictionaryFormatter()
            : this(null)
        {

        }

        public SortedDictionaryFormatter(IComparer<TKey>? comparer)
        {
            this.comparer = comparer;
        }

        [Preserve]
        public override void Serialize(ref MemoryPackWriter writer, ref SortedDictionary<TKey, TValue?>? value)
        {
            if (value == null)
            {
                writer.WriteNullCollectionHeader();
                return;
            }

            var keyFormatter = writer.GetFormatter<TKey>();
            var valueFormatter = writer.GetFormatter<TValue>();

            writer.WriteCollectionHeader(value.Count);
            foreach (var item in value)
            {
                KeyValuePairFormatter.Serialize(keyFormatter, valueFormatter, ref writer, item!);
            }
        }

        [Preserve]
        public override void Deserialize(ref MemoryPackReader reader, ref SortedDictionary<TKey, TValue?>? value)
        {
            if (!reader.TryReadCollectionHeader(out var length))
            {
                value = null;
                return;
            }

            if (value == null)
            {
                value = new SortedDictionary<TKey, TValue?>(comparer);
            }
            else
            {
                value.Clear();
            }

            var keyFormatter = reader.GetFormatter<TKey>();
            var valueFormatter = reader.GetFormatter<TValue>();
            for (int i = 0; i < length; i++)
            {
                KeyValuePairFormatter.Deserialize(keyFormatter, valueFormatter, ref reader, out var k, out var v);
                value.Add(k!, v);
            }
        }
    }

    [Preserve]
    public sealed class SortedListFormatter<TKey, TValue> : MemoryPackFormatter<SortedList<TKey, TValue?>>
        where TKey : notnull
    {
        readonly IComparer<TKey>? comparer;

        public SortedListFormatter()
            : this(null)
        {

        }

        public SortedListFormatter(IComparer<TKey>? comparer)
        {
            this.comparer = comparer;
        }

        [Preserve]
        public override void Serialize(ref MemoryPackWriter writer, ref SortedList<TKey, TValue?>? value)
        {
            if (value == null)
            {
                writer.WriteNullCollectionHeader();
                return;
            }

            var keyFormatter = writer.GetFormatter<TKey>();
            var valueFormatter = writer.GetFormatter<TValue>();

            writer.WriteCollectionHeader(value.Count);
            foreach (var item in value)
            {
                KeyValuePairFormatter.Serialize(keyFormatter, valueFormatter, ref writer, item!);
            }
        }

        [Preserve]
        public override void Deserialize(ref MemoryPackReader reader, ref SortedList<TKey, TValue?>? value)
        {
            if (!reader.TryReadCollectionHeader(out var length))
            {
                value = null;
                return;
            }

            if (value == null)
            {
                value = new SortedList<TKey, TValue?>(length, comparer);
            }
            else
            {
                value.Clear();
            }

            var keyFormatter = reader.GetFormatter<TKey>();
            var valueFormatter = reader.GetFormatter<TValue>();
            for (int i = 0; i < length; i++)
            {
                KeyValuePairFormatter.Deserialize(keyFormatter, valueFormatter, ref reader, out var k, out var v);
                value.Add(k!, v);
            }
        }
    }

    [Preserve]
    public sealed class ConcurrentDictionaryFormatter<TKey, TValue> : MemoryPackFormatter<ConcurrentDictionary<TKey, TValue?>>
        where TKey : notnull
    {
        readonly IEqualityComparer<TKey>? equalityComparer;

        public ConcurrentDictionaryFormatter()
            : this(null)
        {

        }

        public ConcurrentDictionaryFormatter(IEqualityComparer<TKey>? equalityComparer)
        {
            this.equalityComparer = equalityComparer;
        }

        [Preserve]
        public override void Serialize(ref MemoryPackWriter writer, ref ConcurrentDictionary<TKey, TValue?>? value)
        {
            if (value == null)
            {
                writer.WriteNullCollectionHeader();
                return;
            }

            var keyFormatter = writer.GetFormatter<TKey>();
            var valueFormatter = writer.GetFormatter<TValue>();

            var count = value.Count;
            writer.WriteCollectionHeader(count);
            var i = 0;
            foreach (var item in value)
            {
                i++;
                KeyValuePairFormatter.Serialize(keyFormatter, valueFormatter, ref writer, item!);
            }

            if (i != count) MemoryPackSerializationException.ThrowInvalidConcurrrentCollectionOperation();
        }

        [Preserve]
        public override void Deserialize(ref MemoryPackReader reader, ref ConcurrentDictionary<TKey, TValue?>? value)
        {
            if (!reader.TryReadCollectionHeader(out var length))
            {
                value = null;
                return;
            }

            if (value == null)
            {
                value = new ConcurrentDictionary<TKey, TValue?>(equalityComparer);
            }
            else
            {
                value.Clear();
            }

            var keyFormatter = reader.GetFormatter<TKey>();
            var valueFormatter = reader.GetFormatter<TValue>();
            for (int i = 0; i < length; i++)
            {
                KeyValuePairFormatter.Deserialize(keyFormatter, valueFormatter, ref reader, out var k, out var v);
                value.TryAdd(k!, v);
            }
        }
    }

    [Preserve]
    public sealed class ReadOnlyCollectionFormatter<T> : MemoryPackFormatter<ReadOnlyCollection<T?>>
    {
        [Preserve]
        public override void Serialize(ref MemoryPackWriter writer, ref ReadOnlyCollection<T?>? value)
        {
            if (value == null)
            {
                writer.WriteNullCollectionHeader();
                return;
            }

            var formatter = writer.GetFormatter<T?>();
            writer.WriteCollectionHeader(value.Count);
            foreach (var item in value)
            {
                var v = item;
                formatter.Serialize(ref writer, ref v);
            }
        }

        [Preserve]
        public override void Deserialize(ref MemoryPackReader reader, ref ReadOnlyCollection<T?>? value)
        {
            var array = reader.ReadArray<T?>();

            if (array == null)
            {
                value = null;
            }
            else
            {
                value = new ReadOnlyCollection<T?>(array);
            }
        }
    }

    [Preserve]
    public sealed class ReadOnlyObservableCollectionFormatter<T> : MemoryPackFormatter<ReadOnlyObservableCollection<T?>>
    {
        [Preserve]
        public override void Serialize(ref MemoryPackWriter writer, ref ReadOnlyObservableCollection<T?>? value)
        {
            if (value == null)
            {
                writer.WriteNullCollectionHeader();
                return;
            }

            var formatter = writer.GetFormatter<T?>();
            writer.WriteCollectionHeader(value.Count);
            foreach (var item in value)
            {
                var v = item;
                formatter.Serialize(ref writer, ref v);
            }
        }

        [Preserve]
        public override void Deserialize(ref MemoryPackReader reader, ref ReadOnlyObservableCollection<T?>? value)
        {
            var array = reader.ReadArray<T?>();

            if (array == null)
            {
                value = null;
            }
            else
            {
                value = new ReadOnlyObservableCollection<T?>(new ObservableCollection<T?>(array));
            }
        }
    }

    [Preserve]
    public sealed class BlockingCollectionFormatter<T> : MemoryPackFormatter<BlockingCollection<T?>>
    {
        [Preserve]
        public override void Serialize(ref MemoryPackWriter writer, ref BlockingCollection<T?>? value)
        {
            if (value == null)
            {
                writer.WriteNullCollectionHeader();
                return;
            }

            var formatter = writer.GetFormatter<T?>();
            writer.WriteCollectionHeader(value.Count);
            foreach (var item in value)
            {
                var v = item;
                formatter.Serialize(ref writer, ref v);
            }
        }

        [Preserve]
        public override void Deserialize(ref MemoryPackReader reader, ref BlockingCollection<T?>? value)
        {
            if (!reader.TryReadCollectionHeader(out var length))
            {
                value = null;
                return;
            }

            value = new BlockingCollection<T?>();

            var formatter = reader.GetFormatter<T?>();
            for (int i = 0; i < length; i++)
            {
                T? v = default;
                formatter.Deserialize(ref reader, ref v);
                value.Add(v);
            }
        }
    }
}
