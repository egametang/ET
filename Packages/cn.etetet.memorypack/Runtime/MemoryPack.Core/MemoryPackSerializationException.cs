using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

#nullable enable
using System.Buffers;
using System.Diagnostics.CodeAnalysis;

namespace MemoryPack {

public class MemoryPackSerializationException : Exception
{
    public MemoryPackSerializationException(string message)
        : base(message)
    {
    }

    public MemoryPackSerializationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    [DoesNotReturn]
    public static void ThrowMessage(string message)
    {
        throw new MemoryPackSerializationException(message);
    }

    [DoesNotReturn]
    public static void ThrowInvalidPropertyCount(byte expected, byte actual)
    {
        throw new MemoryPackSerializationException($"Current object's property count is {expected} but binary's header maked as {actual}, can't deserialize about versioning.");
    }

    [DoesNotReturn]
    public static void ThrowInvalidPropertyCount(Type type, byte expected, byte actual)
    {
        throw new MemoryPackSerializationException($"{type.FullName} property count is {expected} but binary's header maked as {actual}, can't deserialize about versioning.");
    }

    [DoesNotReturn]
    public static void ThrowInvalidCollection()
    {
        throw new MemoryPackSerializationException($"Current read to collection, the buffer header is not collection.");
    }

    [DoesNotReturn]
    public static void ThrowInvalidRange(int expected, int actual)
    {
        throw new MemoryPackSerializationException($"Requires size is {expected} but buffer length is {actual}.");
    }

    [DoesNotReturn]
    public static void ThrowInvalidAdvance()
    {
        throw new MemoryPackSerializationException($"Cannot advance past the end of the buffer.");
    }

    [DoesNotReturn]
    public static void ThrowSequenceReachedEnd()
    {
        throw new MemoryPackSerializationException($"Sequence reached end, reader can not provide more buffer.");
    }

    [DoesNotReturn]
    public static void ThrowWriteInvalidMemberCount(byte memberCount)
    {
        throw new MemoryPackSerializationException($"MemberCount/Tag allows < 250 but try to write {memberCount}.");
    }

    [DoesNotReturn]
    public static void ThrowInsufficientBufferUnless(int length)
    {
        throw new MemoryPackSerializationException($"Length header size is larger than buffer size, length: {length}.");
    }

    [DoesNotReturn]
    public static void ThrowNotRegisteredInProvider(Type type)
    {
        throw new MemoryPackSerializationException($"{type.FullName} is not registered in this provider.");
    }

    [DoesNotReturn]
    public static void ThrowRegisterInProviderFailed(Type type, Exception innerException)
    {
        throw new MemoryPackSerializationException($"{type.FullName} is failed in provider at creating formatter.", innerException);
    }

    [DoesNotReturn]
    public static void ThrowNotFoundInUnionType(Type actualType, Type baseType)
    {
        throw new MemoryPackSerializationException($"Type {actualType.FullName} is not annotated in {baseType.FullName} MemoryPackUnion.");
    }

    [DoesNotReturn]
    public static void ThrowInvalidTag(ushort tag, Type baseType)
    {
        throw new MemoryPackSerializationException($"Data read tag: {tag} but not found in {baseType.FullName} MemoryPackUnion annotations.");
    }

    [DoesNotReturn]
    public static void ThrowReachedDepthLimit(Type type)
    {
        throw new MemoryPackSerializationException($"Serializing Type '{type}' reached depth limit, maybe detect circular reference.");
    }

    [DoesNotReturn]
    public static void ThrowInvalidConcurrrentCollectionOperation()
    {
        throw new MemoryPackSerializationException($"ConcurrentCollection is Added/Removed in serializing, however serialize concurrent collection is not thread-safe.");
    }

    [DoesNotReturn]
    public static void ThrowDeserializeObjectIsNull(string target)
    {
        throw new MemoryPackSerializationException($"Deserialized {target} is null.");
    }

    [DoesNotReturn]
    public static void ThrowFailedEncoding(OperationStatus status)
    {
        throw new MemoryPackSerializationException($"Failed in Utf8 encoding/decoding process, status: {status}.");
    }

    [DoesNotReturn]
    public static void ThrowCompressionFailed(OperationStatus status)
    {
        throw new MemoryPackSerializationException($"Failed in Brotli compression/decompression process, status: {status}.");
    }

    [DoesNotReturn]
    public static void ThrowCompressionFailed()
    {
        throw new MemoryPackSerializationException($"Failed in Brotli compression/decompression process.");
    }

    [DoesNotReturn]
    public static void ThrowAlreadyDecompressed()
    {
        throw new MemoryPackSerializationException($"BrotliDecompressor can not invoke Decompress twice, already invoked.");
    }

    [DoesNotReturn]
    public static void ThrowDecompressionSizeLimitExceeded(int limit, int size)
    {
        throw new MemoryPackSerializationException($"In decompress process, limit is {limit} but target size is {size}.");
    }
}

}