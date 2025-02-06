using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

#nullable enable
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
#if NET7_0_OR_GREATER
using System.Text.Unicode;
#endif

namespace MemoryPack {

#if NET7_0_OR_GREATER
using static GC;
using static MemoryMarshal;
#else
using static MemoryPack.Internal.MemoryMarshalEx;
#endif

[StructLayout(LayoutKind.Auto)]
public ref partial struct MemoryPackReader
{
    ReadOnlySequence<byte> bufferSource;
    readonly long totalLength;
#if NET7_0_OR_GREATER
    ref byte bufferReference;
#else
    ReadOnlySpan<byte> bufferReference;
#endif
    int bufferLength;
    byte[]? rentBuffer;
    int advancedCount;
    int consumed;   // total length of consumed
    readonly MemoryPackReaderOptionalState optionalState;

    public int Consumed => consumed;
    public long Remaining => totalLength - consumed;
    public MemoryPackReaderOptionalState OptionalState => optionalState;
    public MemoryPackSerializerOptions Options => optionalState.Options;

    public MemoryPackReader(in ReadOnlySequence<byte> sequence, MemoryPackReaderOptionalState optionalState)
    {
        this.bufferSource = sequence.IsSingleSegment ? ReadOnlySequence<byte>.Empty : sequence;
        var span = sequence.FirstSpan;
#if NET7_0_OR_GREATER
        this.bufferReference = ref MemoryMarshal.GetReference(span);
#else
        this.bufferReference = span;
#endif
        this.bufferLength = span.Length;
        this.advancedCount = 0;
        this.consumed = 0;
        this.rentBuffer = null;
        this.totalLength = sequence.Length;
        this.optionalState = optionalState;
    }

    public MemoryPackReader(ReadOnlySpan<byte> buffer, MemoryPackReaderOptionalState optionalState)
    {
        this.bufferSource = ReadOnlySequence<byte>.Empty;
#if NET7_0_OR_GREATER
        this.bufferReference = ref MemoryMarshal.GetReference(buffer);
#else
        this.bufferReference = buffer;
#endif
        this.bufferLength = buffer.Length;
        this.advancedCount = 0;
        this.consumed = 0;
        this.rentBuffer = null;
        this.totalLength = buffer.Length;
        this.optionalState = optionalState;
    }

    // buffer operations

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref byte GetSpanReference(int sizeHint)
    {
        if (sizeHint <= bufferLength)
        {
#if NET7_0_OR_GREATER
            return ref bufferReference;
#else
            return ref MemoryMarshal.GetReference(bufferReference);
#endif
        }

        return ref GetNextSpan(sizeHint);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    ref byte GetNextSpan(int sizeHint)
    {
        if (rentBuffer != null)
        {
            ArrayPool<byte>.Shared.Return(rentBuffer);
            rentBuffer = null;
        }

        if (Remaining == 0)
        {
            MemoryPackSerializationException.ThrowSequenceReachedEnd();
        }

        try
        {
            bufferSource = bufferSource.Slice(advancedCount);
        }
        catch (ArgumentOutOfRangeException)
        {
            MemoryPackSerializationException.ThrowSequenceReachedEnd();
        }

        advancedCount = 0;

        if (sizeHint <= Remaining)
        {
            if (sizeHint <= bufferSource.FirstSpan.Length)
            {
#if NET7_0_OR_GREATER
                bufferReference = ref MemoryMarshal.GetReference(bufferSource.FirstSpan);
                bufferLength = bufferSource.FirstSpan.Length;
                return ref bufferReference;
#else
                bufferReference = bufferSource.FirstSpan;
                bufferLength = bufferSource.FirstSpan.Length;
                return ref MemoryMarshal.GetReference(bufferReference);
#endif
            }

            rentBuffer = ArrayPool<byte>.Shared.Rent(sizeHint);
            bufferSource.Slice(0, sizeHint).CopyTo(rentBuffer);
            var span = rentBuffer.AsSpan(0, sizeHint);
#if NET7_0_OR_GREATER
            bufferReference = ref MemoryMarshal.GetReference(span);
            bufferLength = span.Length;
            return ref bufferReference;
#else
            bufferReference = span;
            bufferLength = span.Length;
            return ref MemoryMarshal.GetReference(bufferReference);
#endif
        }

        MemoryPackSerializationException.ThrowSequenceReachedEnd();
#if NET7_0_OR_GREATER
        return ref bufferReference; // dummy.
#else
        return ref MemoryMarshal.GetReference(bufferReference);
#endif
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Advance(int count)
    {
        if (count == 0) return;

        var rest = bufferLength - count;
        if (rest < 0)
        {
            if (TryAdvanceSequence(count))
            {
                return;
            }
        }

        bufferLength = rest;
#if NET7_0_OR_GREATER
        bufferReference = ref Unsafe.Add(ref bufferReference, count);
#else
        bufferReference = bufferReference.Slice(count);
#endif
        advancedCount += count;
        consumed += count;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    bool TryAdvanceSequence(int count)
    {
        var rest = bufferSource.Length - count;
        if (rest < 0)
        {
            MemoryPackSerializationException.ThrowInvalidAdvance();
        }

        bufferSource = bufferSource.Slice(advancedCount + count);
#if NET7_0_OR_GREATER
        bufferReference = ref MemoryMarshal.GetReference(bufferSource.FirstSpan);
#else
        bufferReference = bufferSource.FirstSpan;
#endif
        bufferLength = bufferSource.FirstSpan.Length;
        advancedCount = 0;
        consumed += count;
        return true;
    }

    public void GetRemainingSource(out ReadOnlySpan<byte> singleSource, out ReadOnlySequence<byte> remainingSource)
    {
        if (bufferSource.IsEmpty)
        {
            remainingSource = ReadOnlySequence<byte>.Empty;
#if NET7_0_OR_GREATER
            singleSource = MemoryMarshal.CreateReadOnlySpan(ref bufferReference, bufferLength);
#else
            singleSource = bufferReference;
#endif
            return;
        }
        else
        {
            if (bufferSource.IsSingleSegment)
            {
                remainingSource = ReadOnlySequence<byte>.Empty;
                singleSource = bufferSource.FirstSpan.Slice(advancedCount);
                return;
            }

            singleSource = default;
            remainingSource = bufferSource.Slice(advancedCount);
            if (remainingSource.IsSingleSegment)
            {
                singleSource = remainingSource.FirstSpan;
                remainingSource = ReadOnlySequence<byte>.Empty;
                return;
            }
            return;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Dispose()
    {
        if (rentBuffer != null)
        {
            ArrayPool<byte>.Shared.Return(rentBuffer);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IMemoryPackFormatter GetFormatter(Type type)
    {
        return MemoryPackFormatterProvider.GetFormatter(type);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IMemoryPackFormatter<T> GetFormatter<T>()
    {
        return MemoryPackFormatterProvider.GetFormatter<T>();
    }

    // read methods

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryReadObjectHeader(out byte memberCount)
    {
        memberCount = GetSpanReference(1);
        Advance(1);
        return memberCount != MemoryPackCode.NullObject;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryReadUnionHeader(out ushort tag)
    {
        var firstTag = GetSpanReference(1);
        Advance(1);
        if (firstTag < MemoryPackCode.WideTag)
        {
            tag = firstTag;
            return true;
        }
        else if (firstTag == MemoryPackCode.WideTag)
        {
            ReadUnmanaged(out tag);
            return true;
        }
        else
        {
            tag = 0;
            return false;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryReadCollectionHeader(out int length)
    {
        length = Unsafe.ReadUnaligned<int>(ref GetSpanReference(4));
        Advance(4);

        // If collection-length is larger than buffer-length, it is invalid data.
        if (Remaining < length)
        {
            MemoryPackSerializationException.ThrowInsufficientBufferUnless(length);
        }

        return length != MemoryPackCode.NullCollection;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool PeekIsNull()
    {
        var code = GetSpanReference(1);
        return code == MemoryPackCode.NullObject;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryPeekObjectHeader(out byte memberCount)
    {
        memberCount = GetSpanReference(1);
        return memberCount != MemoryPackCode.NullObject;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryPeekUnionHeader(out ushort tag)
    {
        var firstTag = GetSpanReference(1);
        if (firstTag < MemoryPackCode.WideTag)
        {
            tag = firstTag;
            return true;
        }
        else if (firstTag == MemoryPackCode.WideTag)
        {
            ref var spanRef = ref GetSpanReference(sizeof(ushort) + 1); // skip firstTag
            tag = Unsafe.ReadUnaligned<ushort>(ref Unsafe.Add(ref spanRef, 1));
            return true;
        }
        else
        {
            tag = 0;
            return false;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryPeekCollectionHeader(out int length)
    {
        length = Unsafe.ReadUnaligned<int>(ref GetSpanReference(4));

        // If collection-length is larger than buffer-length, it is invalid data.
        if (Remaining < length)
        {
            MemoryPackSerializationException.ThrowInsufficientBufferUnless(length);
        }

        return length != MemoryPackCode.NullCollection;
    }

    /// <summary>
    /// no validate collection size, be careful to use.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool DangerousTryReadCollectionHeader(out int length)
    {
        length = Unsafe.ReadUnaligned<int>(ref GetSpanReference(4));
        Advance(4);

        return length != MemoryPackCode.NullCollection;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string? ReadString()
    {
        if (!TryReadCollectionHeader(out var length))
        {
            return null;
        }
        if (length == 0)
        {
            return "";
        }

        if (length > 0)
        {
            return ReadUtf16(length);
        }
        else
        {
            return ReadUtf8(length);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    string ReadUtf16(int length)
    {
        var byteCount = checked(length * 2);
        ref var src = ref GetSpanReference(byteCount);

        var str = new string(MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<byte, char>(ref src), length));

        Advance(byteCount);

        return str;
    }

    [MethodImpl(MethodImplOptions.NoInlining)] // non default, no inline
    string ReadUtf8(int utf8Length)
    {
        // (int ~utf8-byte-count, int utf16-length, utf8-bytes)
        // already read utf8 length, but it is complement.

        utf8Length = ~utf8Length;

        ref var spanRef = ref GetSpanReference(utf8Length + 4); // + read utf16 length

        string str;
        var utf16Length = Unsafe.ReadUnaligned<int>(ref spanRef);

        if (utf16Length <= 0)
        {
            var src = MemoryMarshal.CreateReadOnlySpan(ref Unsafe.Add(ref spanRef, 4), utf8Length);
            str = Encoding.UTF8.GetString(src);
        }
        else
        {
            // check malformed utf16Length
            var max = unchecked((Remaining + 1) * 3);
            if (max < 0) max = int.MaxValue;
            if (max < utf16Length)
            {
                MemoryPackSerializationException.ThrowInsufficientBufferUnless(utf8Length);
            }


#if NET7_0_OR_GREATER
            // regular path, know decoded UTF16 length will gets faster decode result
            unsafe
            {
                fixed (byte* p = &Unsafe.Add(ref spanRef, 4))
                {
                    str = string.Create(utf16Length, ((IntPtr)p, utf8Length), static (dest, state) =>
                    {
                        var src = MemoryMarshal.CreateSpan(ref Unsafe.AsRef<byte>((byte*)state.Item1), state.Item2);
                        var status = Utf8.ToUtf16(src, dest, out var bytesRead, out var charsWritten, replaceInvalidSequences: false);
                        if (status != OperationStatus.Done)
                        {
                            MemoryPackSerializationException.ThrowFailedEncoding(status);
                        }
                    });
                }
            }
#else
            var src = MemoryMarshal.CreateReadOnlySpan(ref Unsafe.Add(ref spanRef, 4), utf8Length);
            str = Encoding.UTF8.GetString(src);
#endif
        }

        Advance(utf8Length + 4);

        return str;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T1 ReadUnmanaged<T1>()
        where T1 : unmanaged
    {
        var size = Unsafe.SizeOf<T1>();
        ref var spanRef = ref GetSpanReference(size);
        var value1 = Unsafe.ReadUnaligned<T1>(ref spanRef);
        Advance(size);
        return value1;
    }

#if NET7_0_OR_GREATER

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ReadPackable<T>(ref T? value)
        where T : IMemoryPackable<T>
    {
        T.Deserialize(ref this, ref value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T? ReadPackable<T>()
        where T : IMemoryPackable<T>
    {
        T? value = default;
        T.Deserialize(ref this, ref value);
        return value;
    }

#else

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ReadPackable<T>(ref T? value)
        where T : IMemoryPackable<T>
    {
        ReadValue(ref value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T? ReadPackable<T>()
        where T : IMemoryPackable<T>
    {
        return ReadValue<T>();
    }

#endif

    // non packable, get formatter dynamically.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ReadValue<T>(ref T? value)
    {
        GetFormatter<T>().Deserialize(ref this, ref value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T? ReadValue<T>()
    {
        T? value = default;
        GetFormatter<T>().Deserialize(ref this, ref value);
        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ReadValue(Type type, ref object? value)
    {
        GetFormatter(type).Deserialize(ref this, ref value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public object? ReadValue(Type type)
    {
        object? value = default;
        GetFormatter(type).Deserialize(ref this, ref value);
        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ReadValueWithFormatter<TFormatter, T>(TFormatter formatter, ref T? value)
        where TFormatter : IMemoryPackFormatter<T>
    {
        formatter.Deserialize(ref this, ref value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T? ReadValueWithFormatter<TFormatter, T>(TFormatter formatter)
        where TFormatter : IMemoryPackFormatter<T>
    {
        T? value = default;
        formatter.Deserialize(ref this, ref value);
        return value;
    }

    #region ReadArray/Span

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T?[]? ReadArray<T>()
    {
        T?[]? value = default;
        ReadArray(ref value);
        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ReadArray<T>(ref T?[]? value)
    {
        if (!RuntimeHelpers.IsReferenceOrContainsReferences<T>())
        {
            DangerousReadUnmanagedArray(ref value);
            return;
        }

        if (!TryReadCollectionHeader(out var length))
        {
            value = null;
            return;
        }

        if (length == 0)
        {
            value = Array.Empty<T>();
            return;
        }

        // T[] support overwrite
        if (value == null || value.Length != length)
        {
            value = new T[length];
        }

        var formatter = GetFormatter<T>();
        for (int i = 0; i < length; i++)
        {
            formatter.Deserialize(ref this, ref value[i]);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ReadSpan<T>(ref Span<T?> value)
    {
        if (!RuntimeHelpers.IsReferenceOrContainsReferences<T>())
        {
            DangerousReadUnmanagedSpan(ref value);
            return;
        }

        if (!TryReadCollectionHeader(out var length))
        {
            value = default;
            return;
        }

        if (length == 0)
        {
            value = Array.Empty<T>();
            return;
        }

        if (value.Length != length)
        {
            value = new T[length];
        }

        var formatter = GetFormatter<T>();
        for (int i = 0; i < length; i++)
        {
            formatter.Deserialize(ref this, ref value[i]);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T?[]? ReadPackableArray<T>()
        where T : IMemoryPackable<T>
    {
        T?[]? value = default;
        ReadPackableArray(ref value);
        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ReadPackableArray<T>(ref T?[]? value)
        where T : IMemoryPackable<T>
    {
#if !NET7_0_OR_GREATER
        ReadArray(ref value);
        return;
#else
        if (!RuntimeHelpers.IsReferenceOrContainsReferences<T>())
        {
            DangerousReadUnmanagedArray(ref value);
            return;
        }

        if (!TryReadCollectionHeader(out var length))
        {
            value = null;
            return;
        }

        if (length == 0)
        {
            value = Array.Empty<T>();
            return;
        }

        // T[] support overwrite
        if (value == null || value.Length != length)
        {
            value = new T[length];
        }

        for (int i = 0; i < length; i++)
        {
            T.Deserialize(ref this, ref value[i]);
        }
#endif
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ReadPackableSpan<T>(ref Span<T?> value)
        where T : IMemoryPackable<T>
    {
#if !NET7_0_OR_GREATER
        ReadSpan(ref value);
        return;
#else
        if (!RuntimeHelpers.IsReferenceOrContainsReferences<T>())
        {
            DangerousReadUnmanagedSpan(ref value);
            return;
        }

        if (!TryReadCollectionHeader(out var length))
        {
            value = default;
            return;
        }

        if (length == 0)
        {
            value = Array.Empty<T>();
            return;
        }

        if (value.Length != length)
        {
            value = new T[length];
        }

        for (int i = 0; i < length; i++)
        {
            T.Deserialize(ref this, ref value[i]);
        }
#endif
    }

    #endregion

    #region UnmanagedArray/Span

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T[]? ReadUnmanagedArray<T>()
        where T : unmanaged
    {
        return DangerousReadUnmanagedArray<T>();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ReadUnmanagedArray<T>(ref T[]? value)
        where T : unmanaged
    {
        DangerousReadUnmanagedArray<T>(ref value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ReadUnmanagedSpan<T>(ref Span<T> value)
        where T : unmanaged
    {
        DangerousReadUnmanagedSpan<T>(ref value);
    }

    // T: should be unamanged type
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe T[]? DangerousReadUnmanagedArray<T>()
    {
        if (!TryReadCollectionHeader(out var length))
        {
            return null;
        }

        if (length == 0) return Array.Empty<T>();

        var byteCount = length * Unsafe.SizeOf<T>();
        ref var src = ref GetSpanReference(byteCount);
        var dest = AllocateUninitializedArray<T>(length);
        Unsafe.CopyBlockUnaligned(ref Unsafe.As<T, byte>(ref GetArrayDataReference(dest)), ref src, (uint)byteCount);
        Advance(byteCount);

        return dest;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe void DangerousReadUnmanagedArray<T>(ref T[]? value)
    {
        if (!TryReadCollectionHeader(out var length))
        {
            value = null;
            return;
        }

        if (length == 0)
        {
            value = Array.Empty<T>();
            return;
        }

        var byteCount = length * Unsafe.SizeOf<T>();
        ref var src = ref GetSpanReference(byteCount);

        if (value == null || value.Length != length)
        {
            value = AllocateUninitializedArray<T>(length);
        }

        ref var dest = ref Unsafe.As<T, byte>(ref GetArrayDataReference(value));
        Unsafe.CopyBlockUnaligned(ref dest, ref src, (uint)byteCount);

        Advance(byteCount);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe void DangerousReadUnmanagedSpan<T>(ref Span<T> value)
    {
        if (!TryReadCollectionHeader(out var length))
        {
            value = default;
            return;
        }

        if (length == 0)
        {
            value = Array.Empty<T>();
            return;
        }

        var byteCount = length * Unsafe.SizeOf<T>();
        ref var src = ref GetSpanReference(byteCount);

        if (value == null || value.Length != length)
        {
            value = AllocateUninitializedArray<T>(length);
        }

        ref var dest = ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(value));
        Unsafe.CopyBlockUnaligned(ref dest, ref src, (uint)byteCount);

        Advance(byteCount);
    }

    #endregion

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ReadSpanWithoutReadLengthHeader<T>(int length, ref Span<T?> value)
    {
        if (length == 0)
        {
            value = Array.Empty<T>();
            return;
        }

        if (!RuntimeHelpers.IsReferenceOrContainsReferences<T>())
        {
            if (value.Length != length)
            {
                value = AllocateUninitializedArray<T>(length);
            }

            var byteCount = length * Unsafe.SizeOf<T>();
            ref var src = ref GetSpanReference(byteCount);
            ref var dest = ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(value)!);
            Unsafe.CopyBlockUnaligned(ref dest, ref src, (uint)byteCount);

            Advance(byteCount);
        }
        else
        {
            if (value.Length != length)
            {
                value = new T[length];
            }

            var formatter = GetFormatter<T>();
            for (int i = 0; i < length; i++)
            {
                formatter.Deserialize(ref this, ref value[i]);
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ReadPackableSpanWithoutReadLengthHeader<T>(int length, ref Span<T?> value)
        where T : IMemoryPackable<T>
    {
#if !NET7_0_OR_GREATER
        ReadSpanWithoutReadLengthHeader(length, ref value);
        return;
#else
        if (length == 0)
        {
            value = Array.Empty<T>();
            return;
        }

        if (!RuntimeHelpers.IsReferenceOrContainsReferences<T>())
        {
            if (value.Length != length)
            {
                value = AllocateUninitializedArray<T>(length);
            }

            var byteCount = length * Unsafe.SizeOf<T>();
            ref var src = ref GetSpanReference(byteCount);
            ref var dest = ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(value)!);
            Unsafe.CopyBlockUnaligned(ref dest, ref src, (uint)byteCount);

            Advance(byteCount);
        }
        else
        {
            if (value.Length != length)
            {
                value = new T[length];
            }

            for (int i = 0; i < length; i++)
            {
                T.Deserialize(ref this, ref value[i]);
            }
        }
#endif
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe void DangerousReadUnmanagedSpanView<T>(out bool isNull, out ReadOnlySpan<byte> view)
    {
        if (!TryReadCollectionHeader(out var length))
        {
            isNull = true;
            view = default;
            return;
        }

        isNull = false;

        if (length == 0)
        {
            view = Array.Empty<byte>();
            return;
        }

        var byteCount = length * Unsafe.SizeOf<T>();
        ref var src = ref GetSpanReference(byteCount);

        var span = MemoryMarshal.CreateReadOnlySpan(ref src, byteCount);

        Advance(byteCount);
        view = span; // safe until call next GetSpanReference
    }
}

}