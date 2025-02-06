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
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// interface collection formatters
// IEnumerable, ICollection, IReadOnlyCollection, IList, IReadOnlyList
// IDictionary, IReadOnlyDictionary, ILookup, IGrouping, ISet, IReadOnlySet

namespace MemoryPack
{
    public static partial class MemoryPackFormatterProvider
    {
        static readonly Dictionary<Type, Type> InterfaceCollectionFormatters = new(11)
        {
            { typeof(IEnumerable<>), typeof(InterfaceEnumerableFormatter<>) },
            { typeof(ICollection<>), typeof(InterfaceCollectionFormatter<>) },
            { typeof(IReadOnlyCollection<>), typeof(InterfaceReadOnlyCollectionFormatter<>) },
            { typeof(IList<>), typeof(InterfaceListFormatter<>) },
            { typeof(IReadOnlyList<>), typeof(InterfaceReadOnlyListFormatter<>) },
            { typeof(IDictionary<,>), typeof(InterfaceDictionaryFormatter<,>) },
            { typeof(IReadOnlyDictionary<,>), typeof(InterfaceReadOnlyDictionaryFormatter<,>) },
            { typeof(ILookup<,>), typeof(InterfaceLookupFormatter<,>) },
            { typeof(IGrouping<,>), typeof(InterfaceGroupingFormatter<,>) },
            { typeof(ISet<>), typeof(InterfaceSetFormatter<>) },
#if NET7_0_OR_GREATER
            { typeof(IReadOnlySet<>), typeof(InterfaceReadOnlySetFormatter<>) },
#endif
        };
    }
}

namespace MemoryPack.Formatters
{
    using static InterfaceCollectionFormatterUtils;

    internal static class InterfaceCollectionFormatterUtils
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TrySerializeOptimized<TCollection, TElement>(ref MemoryPackWriter writer, [NotNullWhen(false)] ref TCollection? value)
            where TCollection : IEnumerable<TElement>
#if NET7_0_OR_GREATER
            
#else
            
#endif
        {
            if (value == null)
            {
                writer.WriteNullCollectionHeader();
                return true;
            }

            // optimize for list or array

            if (value is TElement?[] array)
            {
                writer.WriteArray(array);
                return true;
            }

#if NET7_0_OR_GREATER
            if (value is List<TElement?> list)
            {
                writer.WriteSpan(CollectionsMarshal.AsSpan(list));
                return true;
            }
#endif

            return false;
        }

        public static void SerializeCollection<TCollection, TElement>(ref MemoryPackWriter writer, ref TCollection? value)
            where TCollection : ICollection<TElement>
#if NET7_0_OR_GREATER
            
#else
            
#endif
        {
            if (TrySerializeOptimized<TCollection, TElement>(ref writer, ref value)) return;

            var formatter = writer.GetFormatter<TElement>();
            writer.WriteCollectionHeader(value.Count);
            foreach (var item in value)
            {
                var v = item;
                formatter.Serialize(ref writer, ref v!);
            }
        }

        public static void SerializeReadOnlyCollection<TCollection, TElement>(ref MemoryPackWriter writer, ref TCollection? value)
            where TCollection : IReadOnlyCollection<TElement>
#if NET7_0_OR_GREATER
            
#else
            
#endif
        {
            if (TrySerializeOptimized<TCollection, TElement>(ref writer, ref value)) return;

            var formatter = writer.GetFormatter<TElement>();
            writer.WriteCollectionHeader(value.Count);
            foreach (var item in value)
            {
                var v = item;
                formatter.Serialize(ref writer, ref v!);
            }
        }

        public static List<T?>? ReadList<T>(ref MemoryPackReader reader)
        {
            var formatter = reader.GetFormatter<List<T?>>();
            List<T?>? v = default;
            formatter.Deserialize(ref reader, ref v);
            return v;
        }
    }

    [Preserve]
    public sealed class InterfaceEnumerableFormatter<T> : MemoryPackFormatter<IEnumerable<T?>>
    {
        [Preserve]
        public override void Serialize(ref MemoryPackWriter writer, ref IEnumerable<T?>? value)
        {
            if (TrySerializeOptimized<IEnumerable<T?>, T?>(ref writer, ref value)) return;

            if (value.TryGetNonEnumeratedCountEx(out var count))
            {
                var formatter = writer.GetFormatter<T?>();
                writer.WriteCollectionHeader(count);
                foreach (var item in value)
                {
                    var v = item;
                    formatter.Serialize(ref writer, ref v);
                }
            }
            else
            {
                // write to tempbuffer(because we don't know length so can't write header)
                var tempBuffer = ReusableLinkedArrayBufferWriterPool.Rent();
                try
                {
                    var tempWriter = new MemoryPackWriter(ref Unsafe.As<ReusableLinkedArrayBufferWriter, IBufferWriter<byte>>(ref tempBuffer), writer.OptionalState);

                    count = 0;
                    var formatter = writer.GetFormatter<T?>();
                    foreach (var item in value)
                    {
                        count++;
                        var v = item;
                        formatter.Serialize(ref tempWriter, ref v);
                    }

                    tempWriter.Flush();

                    // write to parameter writer.
                    writer.WriteCollectionHeader(count);
                    tempBuffer.WriteToAndReset(ref writer);
                }
                finally
                {
                    ReusableLinkedArrayBufferWriterPool.Return(tempBuffer);
                }
            }
        }

        [Preserve]
        public override void Deserialize(ref MemoryPackReader reader, ref IEnumerable<T?>? value)
        {
            value = reader.ReadArray<T?>();
        }
    }

    [Preserve]
    public sealed class InterfaceCollectionFormatter<T> : MemoryPackFormatter<ICollection<T?>>
    {
        static InterfaceCollectionFormatter()
        {
            if (!MemoryPackFormatterProvider.IsRegistered<List<T?>>())
            {
                MemoryPackFormatterProvider.Register(new ListFormatter<T>());
            }
        }

        [Preserve]
        public override void Serialize(ref MemoryPackWriter writer, ref ICollection<T?>? value)
        {
            SerializeCollection<ICollection<T?>, T?>(ref writer, ref value);
        }

        [Preserve]
        public override void Deserialize(ref MemoryPackReader reader, ref ICollection<T?>? value)
        {
            value = ReadList<T?>(ref reader);
        }
    }

    [Preserve]
    public sealed class InterfaceReadOnlyCollectionFormatter<T> : MemoryPackFormatter<IReadOnlyCollection<T?>>
    {
        static InterfaceReadOnlyCollectionFormatter()
        {
            if (!MemoryPackFormatterProvider.IsRegistered<List<T?>>())
            {
                MemoryPackFormatterProvider.Register(new ListFormatter<T>());
            }
        }

        [Preserve]
        public override void Serialize(ref MemoryPackWriter writer, ref IReadOnlyCollection<T?>? value)
        {
            SerializeReadOnlyCollection<IReadOnlyCollection<T?>, T?>(ref writer, ref value);
        }

        [Preserve]
        public override void Deserialize(ref MemoryPackReader reader, ref IReadOnlyCollection<T?>? value)
        {
            value = ReadList<T?>(ref reader);
        }
    }

    [Preserve]
    public sealed class InterfaceListFormatter<T> : MemoryPackFormatter<IList<T?>>
    {
        static InterfaceListFormatter()
        {
            if (!MemoryPackFormatterProvider.IsRegistered<List<T?>>())
            {
                MemoryPackFormatterProvider.Register(new ListFormatter<T>());
            }
        }

        [Preserve]
        public override void Serialize(ref MemoryPackWriter writer, ref IList<T?>? value)
        {
            SerializeCollection<IList<T?>, T?>(ref writer, ref value);
        }

        [Preserve]
        public override void Deserialize(ref MemoryPackReader reader, ref IList<T?>? value)
        {
            value = ReadList<T?>(ref reader);
        }
    }

    [Preserve]
    public sealed class InterfaceReadOnlyListFormatter<T> : MemoryPackFormatter<IReadOnlyList<T?>>
    {
        static InterfaceReadOnlyListFormatter()
        {
            if (!MemoryPackFormatterProvider.IsRegistered<List<T?>>())
            {
                MemoryPackFormatterProvider.Register(new ListFormatter<T>());
            }
        }

        [Preserve]
        public override void Serialize(ref MemoryPackWriter writer, ref IReadOnlyList<T?>? value)
        {
            SerializeReadOnlyCollection<IReadOnlyList<T?>, T?>(ref writer, ref value);
        }

        [Preserve]
        public override void Deserialize(ref MemoryPackReader reader, ref IReadOnlyList<T?>? value)
        {
            value = ReadList<T?>(ref reader);
        }
    }

    [Preserve]
    public sealed class InterfaceDictionaryFormatter<TKey, TValue> : MemoryPackFormatter<IDictionary<TKey, TValue?>>
        where TKey : notnull
    {
        readonly IEqualityComparer<TKey>? equalityComparer;

        public InterfaceDictionaryFormatter()
            : this(null)
        {

        }

        public InterfaceDictionaryFormatter(IEqualityComparer<TKey>? equalityComparer)
        {
            this.equalityComparer = equalityComparer;
        }

        [Preserve]
        public override void Serialize(ref MemoryPackWriter writer, ref IDictionary<TKey, TValue?>? value)
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
        public override void Deserialize(ref MemoryPackReader reader, ref IDictionary<TKey, TValue?>? value)
        {
            if (!reader.TryReadCollectionHeader(out var length))
            {
                value = null;
                return;
            }

            var dict = new Dictionary<TKey, TValue?>(equalityComparer);

            var keyFormatter = reader.GetFormatter<TKey>();
            var valueFormatter = reader.GetFormatter<TValue>();
            for (int i = 0; i < length; i++)
            {
                KeyValuePairFormatter.Deserialize(keyFormatter, valueFormatter, ref reader, out var k, out var v);
                dict.Add(k!, v);
            }

            value = dict;
        }
    }

    [Preserve]
    public sealed class InterfaceReadOnlyDictionaryFormatter<TKey, TValue> : MemoryPackFormatter<IReadOnlyDictionary<TKey, TValue?>>
        where TKey : notnull
    {
        readonly IEqualityComparer<TKey>? equalityComparer;

        public InterfaceReadOnlyDictionaryFormatter()
            : this(null)
        {

        }

        public InterfaceReadOnlyDictionaryFormatter(IEqualityComparer<TKey>? equalityComparer)
        {
            this.equalityComparer = equalityComparer;
        }

        [Preserve]
        public override void Serialize(ref MemoryPackWriter writer, ref IReadOnlyDictionary<TKey, TValue?>? value)
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
        public override void Deserialize(ref MemoryPackReader reader, ref IReadOnlyDictionary<TKey, TValue?>? value)
        {
            if (!reader.TryReadCollectionHeader(out var length))
            {
                value = null;
                return;
            }

            var dict = new Dictionary<TKey, TValue?>(equalityComparer);

            var keyFormatter = reader.GetFormatter<TKey>();
            var valueFormatter = reader.GetFormatter<TValue>();
            for (int i = 0; i < length; i++)
            {
                KeyValuePairFormatter.Deserialize(keyFormatter, valueFormatter, ref reader, out var k, out var v);
                dict.Add(k!, v);
            }

            value = dict;
        }
    }

    [Preserve]
    public sealed class InterfaceLookupFormatter<TKey, TElement> : MemoryPackFormatter<ILookup<TKey, TElement>>
        where TKey : notnull
    {
        static InterfaceLookupFormatter()
        {
            if (!MemoryPackFormatterProvider.IsRegistered<IGrouping<TKey, TElement>>())
            {
                MemoryPackFormatterProvider.Register(new InterfaceGroupingFormatter<TKey, TElement>());
            }
        }

        readonly IEqualityComparer<TKey>? equalityComparer;

        public InterfaceLookupFormatter()
            : this(null)
        {

        }

        public InterfaceLookupFormatter(IEqualityComparer<TKey>? equalityComparer)
        {
            this.equalityComparer = equalityComparer;
        }


        [Preserve]
        public override void Serialize(ref MemoryPackWriter writer, ref ILookup<TKey, TElement>? value)
        {
            if (value == null)
            {
                writer.WriteNullCollectionHeader();
                return;
            }

            var formatter = writer.GetFormatter<IGrouping<TKey, TElement>>();
            writer.WriteCollectionHeader(value.Count);
            foreach (var item in value)
            {
                var v = item;
                formatter.Serialize(ref writer, ref v);
            }
        }

        [Preserve]
        public override void Deserialize(ref MemoryPackReader reader, ref ILookup<TKey, TElement>? value)
        {
            if (!reader.TryReadCollectionHeader(out var length))
            {
                value = null;
                return;
            }

            var dict = new Dictionary<TKey, IGrouping<TKey, TElement>>(equalityComparer);

            var formatter = reader.GetFormatter<IGrouping<TKey, TElement>>();
            for (int i = 0; i < length; i++)
            {
                IGrouping<TKey, TElement>? item = default;
                formatter.Deserialize(ref reader, ref item);
                if (item != null)
                {
                    dict.Add(item.Key, item);
                }
            }
            value = new Lookup<TKey, TElement>(dict);
        }
    }

    [Preserve]
    public sealed class InterfaceGroupingFormatter<TKey, TElement> : MemoryPackFormatter<IGrouping<TKey, TElement>>
        where TKey : notnull
    {
        // serialize as {key, [collection]}

        static InterfaceGroupingFormatter()
        {
            if (!MemoryPackFormatterProvider.IsRegistered<IEnumerable<TElement>>())
            {
                MemoryPackFormatterProvider.Register(new InterfaceEnumerableFormatter<TElement>());
            }
        }

        [Preserve]
        public override void Serialize(ref MemoryPackWriter writer, ref IGrouping<TKey, TElement>? value)
        {
            if (value == null)
            {
                writer.WriteNullObjectHeader();
                return;
            }

            writer.WriteObjectHeader(2);
            writer.WriteValue(value.Key);
            writer.WriteValue<IEnumerable<TElement>>(value); // write as IEnumerable<TElement>
        }

        [Preserve]
        public override void Deserialize(ref MemoryPackReader reader, ref IGrouping<TKey, TElement>? value)
        {
            if (!reader.TryReadObjectHeader(out var count))
            {
                value = null;
                return;
            }

            if (count != 2) MemoryPackSerializationException.ThrowInvalidPropertyCount(2, count);

            var key = reader.ReadValue<TKey>();
            var values = reader.ReadArray<TElement>() as IEnumerable<TElement>;

            if (key == null) MemoryPackSerializationException.ThrowDeserializeObjectIsNull(nameof(key));
            if (values == null) MemoryPackSerializationException.ThrowDeserializeObjectIsNull(nameof(values));

            value = new Grouping<TKey, TElement>(key, values);

        }
    }

    [Preserve]
    public sealed class InterfaceSetFormatter<T> : MemoryPackFormatter<ISet<T?>>
    {
        static InterfaceSetFormatter()
        {
            if (!MemoryPackFormatterProvider.IsRegistered<HashSet<T>>())
            {
                MemoryPackFormatterProvider.Register(new HashSetFormatter<T>());
            }
        }

        readonly IEqualityComparer<T?>? equalityComparer;

        public InterfaceSetFormatter()
            : this(null)
        {
        }

        public InterfaceSetFormatter(IEqualityComparer<T?>? equalityComparer)
        {
            this.equalityComparer = equalityComparer;
        }

        [Preserve]
        public override void Serialize(ref MemoryPackWriter writer, ref ISet<T?>? value)
        {
            if (value == null)
            {
                writer.WriteNullCollectionHeader();
                return;
            }

            var formatter = writer.GetFormatter<T>();
            writer.WriteCollectionHeader(value.Count);
            foreach (var item in value)
            {
                var v = item;
                formatter.Serialize(ref writer, ref v);
            }
        }

        [Preserve]
        public override void Deserialize(ref MemoryPackReader reader, ref ISet<T?>? value)
        {
            if (!reader.TryReadCollectionHeader(out var length))
            {
                value = null;
                return;
            }

            var set = new HashSet<T?>(length, equalityComparer);

            var formatter = reader.GetFormatter<T>();
            for (int i = 0; i < length; i++)
            {
                T? item = default;
                formatter.Deserialize(ref reader, ref item);
                set.Add(item);
            }

            value = set;
        }
    }

#if NET7_0_OR_GREATER

    [Preserve]
    public sealed class InterfaceReadOnlySetFormatter<T> : MemoryPackFormatter<IReadOnlySet<T?>>
    {
        static InterfaceReadOnlySetFormatter()
        {
            if (!MemoryPackFormatterProvider.IsRegistered<HashSet<T>>())
            {
                MemoryPackFormatterProvider.Register(new HashSetFormatter<T>());
            }
        }

        readonly IEqualityComparer<T?>? equalityComparer;

        public InterfaceReadOnlySetFormatter()
            : this(null)
        {
        }

        public InterfaceReadOnlySetFormatter(IEqualityComparer<T?>? equalityComparer)
        {
            this.equalityComparer = equalityComparer;
        }

        [Preserve]
        public override void Serialize(ref MemoryPackWriter writer, ref IReadOnlySet<T?>? value)
        {
            if (value == null)
            {
                writer.WriteNullCollectionHeader();
                return;
            }

            var formatter = writer.GetFormatter<T>();
            writer.WriteCollectionHeader(value.Count);
            foreach (var item in value)
            {
                var v = item;
                formatter.Serialize(ref writer, ref v);
            }
        }

        [Preserve]
        public override void Deserialize(ref MemoryPackReader reader, ref IReadOnlySet<T?>? value)
        {
            if (!reader.TryReadCollectionHeader(out var length))
            {
                value = null;
                return;
            }

            var set = new HashSet<T?>(length, equalityComparer);

            var formatter = reader.GetFormatter<T>();
            for (int i = 0; i < length; i++)
            {
                T? item = default;
                formatter.Deserialize(ref reader, ref item);
                set.Add(item);
            }

            value = set;
        }
    }

#endif

    [Preserve]
    internal sealed class Grouping<TKey, TElement> : IGrouping<TKey, TElement>
    {
        readonly TKey key;
        readonly IEnumerable<TElement> elements;

        public Grouping(TKey key, IEnumerable<TElement> elements)
        {
            this.key = key;
            this.elements = elements;
        }

        public TKey Key
        {
            get
            {
                return this.key;
            }
        }

        public IEnumerator<TElement> GetEnumerator()
        {
            return this.elements.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.elements.GetEnumerator();
        }
    }

    [Preserve]
    internal sealed class Lookup<TKey, TElement> : ILookup<TKey, TElement>
        where TKey : notnull
    {
        readonly Dictionary<TKey, IGrouping<TKey, TElement>> groupings;

        public Lookup(Dictionary<TKey, IGrouping<TKey, TElement>> groupings)
        {
            this.groupings = groupings;
        }

        public IEnumerable<TElement> this[TKey key]
        {
            get
            {
                return this.groupings[key];
            }
        }

        public int Count
        {
            get
            {
                return this.groupings.Count;
            }
        }

        public bool Contains(TKey key)
        {
            return this.groupings.ContainsKey(key);
        }

        public IEnumerator<IGrouping<TKey, TElement>> GetEnumerator()
        {
            return this.groupings.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.groupings.Values.GetEnumerator();
        }
    }
}
