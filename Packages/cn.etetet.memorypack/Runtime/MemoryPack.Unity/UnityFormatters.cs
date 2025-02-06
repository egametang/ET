#nullable enable

using MemoryPack.Internal;
using UnityEngine;

namespace MemoryPack
{
    [Preserve]
    internal sealed class AnimationCurveFormatter : MemoryPackFormatter<AnimationCurve>
    {
        [Preserve]
        public override void Serialize(ref MemoryPackWriter writer, ref AnimationCurve? value)
        {
            if (value == null)
            {
                writer.WriteNullObjectHeader();
                return;
            }

            writer.WriteUnmanagedWithObjectHeader(3, value.@preWrapMode, value.@postWrapMode);
            writer.WriteUnmanagedArray(value.@keys);
        }

        [Preserve]
        public override void Deserialize(ref MemoryPackReader reader, ref AnimationCurve? value)
        {
            if (!reader.TryReadObjectHeader(out var count))
            {
                value = null;
                return;
            }

            if (count != 3) MemoryPackSerializationException.ThrowInvalidPropertyCount(3, count);

            reader.ReadUnmanaged(out WrapMode preWrapMode, out WrapMode postWrapMode);
            var keys = reader.ReadUnmanagedArray<global::UnityEngine.Keyframe>();

            if (value == null)
            {
                value = new AnimationCurve();
            }

            value.preWrapMode = preWrapMode;
            value.postWrapMode = postWrapMode;
            value.keys = keys;
        }
    }

    [Preserve]
    internal sealed class GradientFormatter : MemoryPackFormatter<Gradient>
    {
        [Preserve]
        public override void Serialize(ref MemoryPackWriter writer, ref Gradient? value)
        {
            if (value == null)
            {
                writer.WriteNullObjectHeader();
                return;
            }

            writer.WriteObjectHeader(3);
            writer.WriteUnmanagedArray(value.@colorKeys);
            writer.WriteUnmanagedArray(value.@alphaKeys);
            writer.WriteUnmanaged(value.@mode);
        }

        [Preserve]
        public override void Deserialize(ref MemoryPackReader reader, ref Gradient? value)
        {
            if (!reader.TryReadObjectHeader(out var count))
            {
                value = null;
                return;
            }

            if (count != 3) MemoryPackSerializationException.ThrowInvalidPropertyCount(3, count);

            var colorKeys = reader.ReadUnmanagedArray<global::UnityEngine.GradientColorKey>();
            var alphaKeys = reader.ReadUnmanagedArray<global::UnityEngine.GradientAlphaKey>();
            reader.ReadUnmanaged(out GradientMode mode);

            if (value == null)
            {
                value = new Gradient();
            }

            value.colorKeys = colorKeys;
            value.alphaKeys = alphaKeys;
            value.mode = mode;
        }
    }

    [Preserve]
    internal sealed class RectOffsetFormatter : MemoryPackFormatter<RectOffset>
    {
        [Preserve]
        public override void Serialize(ref MemoryPackWriter writer, ref RectOffset? value)
        {
            if (value == null)
            {
                writer.WriteNullObjectHeader();
                return;
            }

            writer.WriteUnmanagedWithObjectHeader(4, value.@left, value.@right, value.@top, value.@bottom);
        }

        [Preserve]
        public override void Deserialize(ref MemoryPackReader reader, ref RectOffset? value)
        {
            if (!reader.TryReadObjectHeader(out var count))
            {
                value = null;
                return;
            }

            if (count != 4) MemoryPackSerializationException.ThrowInvalidPropertyCount(4, count);

            reader.ReadUnmanaged(out int left, out int right, out int top, out int bottom);

            if (value == null)
            {
                value = new RectOffset(left, right, top, bottom);
            }
            else
            {
                value.left = left;
                value.right = right;
                value.top = top;
                value.bottom = bottom;
            }
        }
    }
}
