using MemoryPack.Internal;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MemoryPack;
using MemoryPack.Formatters;

#nullable enable
namespace ET
{
    /// <summary>
    /// 替换默认的SortedDictionaryFormatter
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    [Preserve]
    public sealed class MemoryPackSortedDictionaryFormatter<TKey, TValue> : MemoryPackFormatter<SortedDictionary<TKey, TValue?>>
            where TKey : notnull
    {
        readonly IComparer<TKey>? comparer;

        public MemoryPackSortedDictionaryFormatter()
                : this(null)
        {

        }

        public MemoryPackSortedDictionaryFormatter(IComparer<TKey>? comparer)
        {
            this.comparer = comparer;
        }


        [Preserve]
#if UNITY
        public override void Serialize(ref MemoryPackWriter writer, ref SortedDictionary<TKey, TValue?>? value)
#else
        public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref SortedDictionary<TKey, TValue?>? value)
#endif
        {
            if (value == null)
            {
                writer.WriteNullCollectionHeader();
                return;
            }

            var keyFormatter = writer.GetFormatter<TKey>();
            var valueFormatter = writer.GetFormatter<TValue>();

            // writer.WriteCollectionHeader(value.Count);
            ref byte spanReference = ref writer.GetSpanReference(4);
            writer.Advance(4);
            int count = 0;
            foreach (var item in value)
            {
                if (item.Value is not ISerializeToEntity)
                {
                    continue;
                }
                ++count;
                KeyValuePairFormatter.Serialize(keyFormatter, valueFormatter, ref writer, item!);
            }
            Unsafe.WriteUnaligned(ref spanReference, count);
        }

        [Preserve]
#if UNITY
        public override void Deserialize(ref MemoryPackReader reader, ref SortedDictionary<TKey, TValue?>? value)
#else
        
        public override void Deserialize(ref MemoryPackReader reader, scoped ref SortedDictionary<TKey, TValue?>? value)
#endif
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
}