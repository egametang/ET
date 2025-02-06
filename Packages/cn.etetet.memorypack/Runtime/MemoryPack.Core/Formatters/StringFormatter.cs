using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

#nullable enable
using MemoryPack.Compression;
using MemoryPack.Internal;
using System.Buffers;
using System.IO.Compression;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MemoryPack.Formatters {

[Preserve]
public sealed class StringFormatter : MemoryPackFormatter<string>
{
    public static readonly StringFormatter Default = new StringFormatter();

    [Preserve]
    public override void Serialize(ref MemoryPackWriter writer, ref string? value)
    {
        writer.WriteString(value);
    }

    [Preserve]
    public override void Deserialize(ref MemoryPackReader reader, ref string? value)
    {
        value = reader.ReadString();
    }
}

[Preserve]
public sealed class Utf8StringFormatter : MemoryPackFormatter<string>
{
    public static readonly Utf8StringFormatter Default = new Utf8StringFormatter();

    [Preserve]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void Serialize(ref MemoryPackWriter writer, ref string? value)
    {
        writer.WriteUtf8(value);
    }

    [Preserve]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void Deserialize(ref MemoryPackReader reader, ref string? value)
    {
        value = reader.ReadString();
    }
}

[Preserve]
public sealed class Utf16StringFormatter : MemoryPackFormatter<string>
{
    public static readonly Utf16StringFormatter Default = new Utf16StringFormatter();

    [Preserve]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void Serialize(ref MemoryPackWriter writer, ref string? value)
    {
        writer.WriteUtf16(value);
    }

    [Preserve]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void Deserialize(ref MemoryPackReader reader, ref string? value)
    {
        value = reader.ReadString();
    }
}

[Preserve]
public sealed class InternStringFormatter : MemoryPackFormatter<string>
{
    public static readonly InternStringFormatter Default = new InternStringFormatter();

    [Preserve]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void Serialize(ref MemoryPackWriter writer, ref string? value)
    {
        writer.WriteString(value);
    }

    [Preserve]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void Deserialize(ref MemoryPackReader reader, ref string? value)
    {
        var str = reader.ReadString();
        if (str == null)
        {
            value = null;
            return;
        }

        value = string.Intern(str);
    }
}

[Preserve]
public sealed class BrotliStringFormatter : MemoryPackFormatter<string>
{
    [ThreadStatic]
    static StrongBox<int>? threadStaticConsumedBox;

    internal const int DefaultDecompssionSizeLimit = 1024 * 1024 * 128; // 128MB

    public static readonly BrotliStringFormatter Default = new BrotliStringFormatter();

    readonly System.IO.Compression.CompressionLevel compressionLevel;
    readonly int window;
    readonly int decompressionSizeLimit;

    public BrotliStringFormatter()
        : this(System.IO.Compression.CompressionLevel.Fastest)
    {

    }

    public BrotliStringFormatter(System.IO.Compression.CompressionLevel compressionLevel)
        : this(compressionLevel, BrotliUtils.WindowBits_Default)
    {
        this.compressionLevel = compressionLevel;
    }

    public BrotliStringFormatter(System.IO.Compression.CompressionLevel compressionLevel, int window)
        : this(compressionLevel, window, DefaultDecompssionSizeLimit)
    {
    }

    public BrotliStringFormatter(System.IO.Compression.CompressionLevel compressionLevel, int window, int decompressionSizeLimit)
    {
        this.compressionLevel = compressionLevel;
        this.window = window;
        this.decompressionSizeLimit = decompressionSizeLimit;
    }

    [Preserve]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void Serialize(ref MemoryPackWriter writer, ref string? value)
    {
        if (value == null)
        {
            writer.WriteNullCollectionHeader();
            return;
        }

        if (value.Length == 0)
        {
            writer.WriteCollectionHeader(0);
            return;
        }

        var quality = BrotliUtils.GetQualityFromCompressionLevel(compressionLevel);
        using var encoder = new BrotliEncoder(quality, window);

        var srcLength = value.Length * 2;
        var maxLength = BrotliUtils.BrotliEncoderMaxCompressedSize(srcLength);

        ref var spanRef = ref writer.GetSpanReference(maxLength + 4);
        var dest = MemoryMarshal.CreateSpan(ref Unsafe.Add(ref spanRef, 4), maxLength);

        var status = encoder.Compress(MemoryMarshal.AsBytes(value.AsSpan()), dest, out var bytesConsumed, out var bytesWritten, isFinalBlock: true);
        if (status != OperationStatus.Done)
        {
            MemoryPackSerializationException.ThrowCompressionFailed(status);
        }

        if (bytesConsumed != srcLength)
        {
            MemoryPackSerializationException.ThrowCompressionFailed();
        }

        Unsafe.WriteUnaligned(ref spanRef, value.Length);
        writer.Advance(bytesWritten + 4);
    }

    [Preserve]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void Deserialize(ref MemoryPackReader reader, ref string? value)
    {
        if (!reader.DangerousTryReadCollectionHeader(out var length))
        {
            value = null;
            return;
        }

        if (length == 0)
        {
            value = "";
            return;
        }

        var byteLength = length * 2;

        // security, require to check length
        if (decompressionSizeLimit < byteLength)
        {
            MemoryPackSerializationException.ThrowDecompressionSizeLimitExceeded(decompressionSizeLimit, byteLength);
        }

        reader.GetRemainingSource(out var singleSource, out var remainingSource);

        var consumedBox = threadStaticConsumedBox;
        if (consumedBox == null)
        {
            consumedBox = threadStaticConsumedBox = new StrongBox<int>();
        }
        else
        {
            consumedBox.Value = 0;
        }

        if (singleSource.Length != 0)
        {
            unsafe
            {
                fixed (byte* p = singleSource)
                {
                    value = string.Create(length, ((IntPtr)p, singleSource.Length, byteLength, consumedBox), static (stringSpan, state) =>
                    {
                        var src = MemoryMarshal.CreateSpan(ref Unsafe.AsRef<byte>((byte*)state.Item1), state.Item2);
                        var destination = MemoryMarshal.AsBytes(stringSpan);

                        using var decoder = new BrotliDecoder();
                        var status = decoder.Decompress(src, destination, out var bytesConsumed, out var bytesWritten);
                        if (status != OperationStatus.Done)
                        {
                            MemoryPackSerializationException.ThrowCompressionFailed(status);
                        }
                        if (bytesWritten != state.byteLength)
                        {
                            MemoryPackSerializationException.ThrowCompressionFailed();
                        }

                        state.consumedBox.Value = bytesConsumed;
                    });
                    reader.Advance(consumedBox.Value);
                }
            }
        }
        else
        {
            value = string.Create(length, (remainingSource, remainingSource.Length, byteLength, consumedBox), static (stringSpan, state) =>
            {
                var destination = MemoryMarshal.AsBytes(stringSpan);

                using var decoder = new BrotliDecoder();

                var consumed = 0;
                OperationStatus status = OperationStatus.DestinationTooSmall;
                foreach (var item in state.remainingSource)
                {
                    status = decoder.Decompress(item.Span, destination, out var bytesConsumed, out var bytesWritten);
                    consumed += bytesConsumed;

                    destination = destination.Slice(bytesWritten);
                    if (status == OperationStatus.Done)
                    {
                        break;
                    }
                }
                if (status != OperationStatus.Done)
                {
                    MemoryPackSerializationException.ThrowCompressionFailed(status);
                }

                state.consumedBox.Value = consumed;
            });
            reader.Advance(consumedBox.Value);
        }
    }
}

}