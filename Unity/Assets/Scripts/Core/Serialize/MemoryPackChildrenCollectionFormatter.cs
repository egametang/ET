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
    [Preserve]
    public sealed class MemoryPackChildrenCollectionFormatter : MemoryPackFormatter<ChildrenCollection>
    {
        [Preserve]
#if UNITY
        public override void Serialize(ref MemoryPackWriter writer, ref ChildrenCollection? value)
#else
        public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref ChildrenCollection? value)
#endif
        {
            if (value == null)
            {
                writer.WriteNullCollectionHeader();
                return;
            }
            
            // writer.WriteCollectionHeader(value.Count);
            var formatter = writer.GetFormatter<Entity>();
            ref byte spanReference = ref writer.GetSpanReference(4);
            writer.Advance(4);
            int count = 0;
            foreach (var kv in value)
            {
                if (kv.Value is not ISerializeToEntity)
                {
                    continue;
                }

                Entity entity = kv.Value;
                ++count;
                formatter.Serialize(ref writer, ref entity!);
            }
            Unsafe.WriteUnaligned(ref spanReference, count);
        }

        [Preserve]
#if UNITY
        public override void Deserialize(ref MemoryPackReader reader, ref ChildrenCollection? value)
#else
        public override void Deserialize(ref MemoryPackReader reader, scoped ref ChildrenCollection? value)
#endif
        {
            if (!reader.TryReadCollectionHeader(out int length))
            {
                value = null;
                return;
            }

            if (value == null)
            {
                value = new ChildrenCollection();
            }
            else
            {
                value.Clear();
            }

            var formatter = reader.GetFormatter<Entity>();
            for (int i = 0; i < length; i++)
            {
                Entity entity = null!;
                formatter.Deserialize(ref reader, ref entity!);
                value.Add(entity.Id, entity);
            }
        }
    }
}