using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

#nullable enable
using MemoryPack.Internal;

namespace MemoryPack.Formatters {

internal static class TupleFormatterTypes
{
    public static readonly Dictionary<Type, Type> TupleFormatters = new Dictionary<Type, Type>(16)
    {
        { typeof(Tuple<>), typeof(TupleFormatter<>) },
        { typeof(ValueTuple<>), typeof(ValueTupleFormatter<>) },
        { typeof(Tuple<,>), typeof(TupleFormatter<,>) },
        { typeof(ValueTuple<,>), typeof(ValueTupleFormatter<,>) },
        { typeof(Tuple<,,>), typeof(TupleFormatter<,,>) },
        { typeof(ValueTuple<,,>), typeof(ValueTupleFormatter<,,>) },
        { typeof(Tuple<,,,>), typeof(TupleFormatter<,,,>) },
        { typeof(ValueTuple<,,,>), typeof(ValueTupleFormatter<,,,>) },
        { typeof(Tuple<,,,,>), typeof(TupleFormatter<,,,,>) },
        { typeof(ValueTuple<,,,,>), typeof(ValueTupleFormatter<,,,,>) },
        { typeof(Tuple<,,,,,>), typeof(TupleFormatter<,,,,,>) },
        { typeof(ValueTuple<,,,,,>), typeof(ValueTupleFormatter<,,,,,>) },
        { typeof(Tuple<,,,,,,>), typeof(TupleFormatter<,,,,,,>) },
        { typeof(ValueTuple<,,,,,,>), typeof(ValueTupleFormatter<,,,,,,>) },
        { typeof(Tuple<,,,,,,,>), typeof(TupleFormatter<,,,,,,,>) },
        { typeof(ValueTuple<,,,,,,,>), typeof(ValueTupleFormatter<,,,,,,,>) },
    };
}

[Preserve]
public sealed class TupleFormatter<T1> : MemoryPackFormatter<Tuple<T1?>>
{
    [Preserve]
    public override void Serialize(ref MemoryPackWriter writer, ref Tuple<T1?>? value)
    {
        if (value == null)
        {
            writer.WriteNullObjectHeader();
            return;
        }

        writer.WriteObjectHeader(1);
        writer.WriteValue(value.Item1);
    }

    [Preserve]
    public override void Deserialize(ref MemoryPackReader reader, ref Tuple<T1?>? value)
    {
        if (!reader.TryReadObjectHeader(out var count))
        {
            value = null;
            return;
        }

        if (count != 1) MemoryPackSerializationException.ThrowInvalidPropertyCount(1, count);

        value = new Tuple<T1?>(
            reader.ReadValue<T1>()
        );
    }
}

[Preserve]
public sealed class TupleFormatter<T1, T2> : MemoryPackFormatter<Tuple<T1?, T2?>>
{
    [Preserve]
    public override void Serialize(ref MemoryPackWriter writer, ref Tuple<T1?, T2?>? value)
    {
        if (value == null)
        {
            writer.WriteNullObjectHeader();
            return;
        }

        writer.WriteObjectHeader(2);
        writer.WriteValue(value.Item1);
        writer.WriteValue(value.Item2);
    }

    [Preserve]
    public override void Deserialize(ref MemoryPackReader reader, ref Tuple<T1?, T2?>? value)
    {
        if (!reader.TryReadObjectHeader(out var count))
        {
            value = null;
            return;
        }

        if (count != 2) MemoryPackSerializationException.ThrowInvalidPropertyCount(2, count);

        value = new Tuple<T1?, T2?>(
            reader.ReadValue<T1>(),
            reader.ReadValue<T2>()
        );
    }
}

[Preserve]
public sealed class TupleFormatter<T1, T2, T3> : MemoryPackFormatter<Tuple<T1?, T2?, T3?>>
{
    [Preserve]
    public override void Serialize(ref MemoryPackWriter writer, ref Tuple<T1?, T2?, T3?>? value)
    {
        if (value == null)
        {
            writer.WriteNullObjectHeader();
            return;
        }

        writer.WriteObjectHeader(3);
        writer.WriteValue(value.Item1);
        writer.WriteValue(value.Item2);
        writer.WriteValue(value.Item3);
    }

    [Preserve]
    public override void Deserialize(ref MemoryPackReader reader, ref Tuple<T1?, T2?, T3?>? value)
    {
        if (!reader.TryReadObjectHeader(out var count))
        {
            value = null;
            return;
        }

        if (count != 3) MemoryPackSerializationException.ThrowInvalidPropertyCount(3, count);

        value = new Tuple<T1?, T2?, T3?>(
            reader.ReadValue<T1>(),
            reader.ReadValue<T2>(),
            reader.ReadValue<T3>()
        );
    }
}

[Preserve]
public sealed class TupleFormatter<T1, T2, T3, T4> : MemoryPackFormatter<Tuple<T1?, T2?, T3?, T4?>>
{
    [Preserve]
    public override void Serialize(ref MemoryPackWriter writer, ref Tuple<T1?, T2?, T3?, T4?>? value)
    {
        if (value == null)
        {
            writer.WriteNullObjectHeader();
            return;
        }

        writer.WriteObjectHeader(4);
        writer.WriteValue(value.Item1);
        writer.WriteValue(value.Item2);
        writer.WriteValue(value.Item3);
        writer.WriteValue(value.Item4);
    }

    [Preserve]
    public override void Deserialize(ref MemoryPackReader reader, ref Tuple<T1?, T2?, T3?, T4?>? value)
    {
        if (!reader.TryReadObjectHeader(out var count))
        {
            value = null;
            return;
        }

        if (count != 4) MemoryPackSerializationException.ThrowInvalidPropertyCount(4, count);

        value = new Tuple<T1?, T2?, T3?, T4?>(
            reader.ReadValue<T1>(),
            reader.ReadValue<T2>(),
            reader.ReadValue<T3>(),
            reader.ReadValue<T4>()
        );
    }
}

[Preserve]
public sealed class TupleFormatter<T1, T2, T3, T4, T5> : MemoryPackFormatter<Tuple<T1?, T2?, T3?, T4?, T5?>>
{
    [Preserve]
    public override void Serialize(ref MemoryPackWriter writer, ref Tuple<T1?, T2?, T3?, T4?, T5?>? value)
    {
        if (value == null)
        {
            writer.WriteNullObjectHeader();
            return;
        }

        writer.WriteObjectHeader(5);
        writer.WriteValue(value.Item1);
        writer.WriteValue(value.Item2);
        writer.WriteValue(value.Item3);
        writer.WriteValue(value.Item4);
        writer.WriteValue(value.Item5);
    }

    [Preserve]
    public override void Deserialize(ref MemoryPackReader reader, ref Tuple<T1?, T2?, T3?, T4?, T5?>? value)
    {
        if (!reader.TryReadObjectHeader(out var count))
        {
            value = null;
            return;
        }

        if (count != 5) MemoryPackSerializationException.ThrowInvalidPropertyCount(5, count);

        value = new Tuple<T1?, T2?, T3?, T4?, T5?>(
            reader.ReadValue<T1>(),
            reader.ReadValue<T2>(),
            reader.ReadValue<T3>(),
            reader.ReadValue<T4>(),
            reader.ReadValue<T5>()
        );
    }
}

[Preserve]
public sealed class TupleFormatter<T1, T2, T3, T4, T5, T6> : MemoryPackFormatter<Tuple<T1?, T2?, T3?, T4?, T5?, T6?>>
{
    [Preserve]
    public override void Serialize(ref MemoryPackWriter writer, ref Tuple<T1?, T2?, T3?, T4?, T5?, T6?>? value)
    {
        if (value == null)
        {
            writer.WriteNullObjectHeader();
            return;
        }

        writer.WriteObjectHeader(6);
        writer.WriteValue(value.Item1);
        writer.WriteValue(value.Item2);
        writer.WriteValue(value.Item3);
        writer.WriteValue(value.Item4);
        writer.WriteValue(value.Item5);
        writer.WriteValue(value.Item6);
    }

    [Preserve]
    public override void Deserialize(ref MemoryPackReader reader, ref Tuple<T1?, T2?, T3?, T4?, T5?, T6?>? value)
    {
        if (!reader.TryReadObjectHeader(out var count))
        {
            value = null;
            return;
        }

        if (count != 6) MemoryPackSerializationException.ThrowInvalidPropertyCount(6, count);

        value = new Tuple<T1?, T2?, T3?, T4?, T5?, T6?>(
            reader.ReadValue<T1>(),
            reader.ReadValue<T2>(),
            reader.ReadValue<T3>(),
            reader.ReadValue<T4>(),
            reader.ReadValue<T5>(),
            reader.ReadValue<T6>()
        );
    }
}

[Preserve]
public sealed class TupleFormatter<T1, T2, T3, T4, T5, T6, T7> : MemoryPackFormatter<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?>>
{
    [Preserve]
    public override void Serialize(ref MemoryPackWriter writer, ref Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?>? value)
    {
        if (value == null)
        {
            writer.WriteNullObjectHeader();
            return;
        }

        writer.WriteObjectHeader(7);
        writer.WriteValue(value.Item1);
        writer.WriteValue(value.Item2);
        writer.WriteValue(value.Item3);
        writer.WriteValue(value.Item4);
        writer.WriteValue(value.Item5);
        writer.WriteValue(value.Item6);
        writer.WriteValue(value.Item7);
    }

    [Preserve]
    public override void Deserialize(ref MemoryPackReader reader, ref Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?>? value)
    {
        if (!reader.TryReadObjectHeader(out var count))
        {
            value = null;
            return;
        }

        if (count != 7) MemoryPackSerializationException.ThrowInvalidPropertyCount(7, count);

        value = new Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?>(
            reader.ReadValue<T1>(),
            reader.ReadValue<T2>(),
            reader.ReadValue<T3>(),
            reader.ReadValue<T4>(),
            reader.ReadValue<T5>(),
            reader.ReadValue<T6>(),
            reader.ReadValue<T7>()
        );
    }
}

[Preserve]
public sealed class TupleFormatter<T1, T2, T3, T4, T5, T6, T7, TRest> : MemoryPackFormatter<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, TRest>>
    where TRest : notnull
{
    [Preserve]
    public override void Serialize(ref MemoryPackWriter writer, ref Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, TRest>? value)
    {
        if (value == null)
        {
            writer.WriteNullObjectHeader();
            return;
        }

        writer.WriteObjectHeader(8);
        writer.WriteValue(value.Item1);
        writer.WriteValue(value.Item2);
        writer.WriteValue(value.Item3);
        writer.WriteValue(value.Item4);
        writer.WriteValue(value.Item5);
        writer.WriteValue(value.Item6);
        writer.WriteValue(value.Item7);
        writer.WriteValue(value.Rest);
    }

    [Preserve]
    public override void Deserialize(ref MemoryPackReader reader, ref Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, TRest>? value)
    {
        if (!reader.TryReadObjectHeader(out var count))
        {
            value = null;
            return;
        }

        if (count != 8) MemoryPackSerializationException.ThrowInvalidPropertyCount(8, count);

        value = new Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, TRest>(
            reader.ReadValue<T1>(),
            reader.ReadValue<T2>(),
            reader.ReadValue<T3>(),
            reader.ReadValue<T4>(),
            reader.ReadValue<T5>(),
            reader.ReadValue<T6>(),
            reader.ReadValue<T7>(),
            reader.ReadValue<TRest>()!
        );
    }
}


[Preserve]
public sealed class ValueTupleFormatter<T1> : MemoryPackFormatter<ValueTuple<T1?>>
{
    [Preserve]
    public override void Serialize(ref MemoryPackWriter writer, ref ValueTuple<T1?> value)
    {
        if (!System.Runtime.CompilerServices.RuntimeHelpers.IsReferenceOrContainsReferences<ValueTuple<T1?>>())
        {
            writer.DangerousWriteUnmanaged(value);
            return;
        }

        writer.WriteValue(value.Item1);
    }

    [Preserve]
    public override void Deserialize(ref MemoryPackReader reader, ref ValueTuple<T1?> value)
    {
        if (!System.Runtime.CompilerServices.RuntimeHelpers.IsReferenceOrContainsReferences<ValueTuple<T1?>>())
        {
            reader.DangerousReadUnmanaged(out value);
            return;
        }

        value = new ValueTuple<T1?>(
            reader.ReadValue<T1>()
        );
    }
}

[Preserve]
public sealed class ValueTupleFormatter<T1, T2> : MemoryPackFormatter<ValueTuple<T1?, T2?>>
{
    [Preserve]
    public override void Serialize(ref MemoryPackWriter writer, ref ValueTuple<T1?, T2?> value)
    {
        if (!System.Runtime.CompilerServices.RuntimeHelpers.IsReferenceOrContainsReferences<ValueTuple<T1?, T2?>>())
        {
            writer.DangerousWriteUnmanaged(value);
            return;
        }

        writer.WriteValue(value.Item1);
        writer.WriteValue(value.Item2);
    }

    [Preserve]
    public override void Deserialize(ref MemoryPackReader reader, ref ValueTuple<T1?, T2?> value)
    {
        if (!System.Runtime.CompilerServices.RuntimeHelpers.IsReferenceOrContainsReferences<ValueTuple<T1?, T2?>>())
        {
            reader.DangerousReadUnmanaged(out value);
            return;
        }

        value = new ValueTuple<T1?, T2?>(
            reader.ReadValue<T1>(),
            reader.ReadValue<T2>()
        );
    }
}

[Preserve]
public sealed class ValueTupleFormatter<T1, T2, T3> : MemoryPackFormatter<ValueTuple<T1?, T2?, T3?>>
{
    [Preserve]
    public override void Serialize(ref MemoryPackWriter writer, ref ValueTuple<T1?, T2?, T3?> value)
    {
        if (!System.Runtime.CompilerServices.RuntimeHelpers.IsReferenceOrContainsReferences<ValueTuple<T1?, T2?, T3?>>())
        {
            writer.DangerousWriteUnmanaged(value);
            return;
        }

        writer.WriteValue(value.Item1);
        writer.WriteValue(value.Item2);
        writer.WriteValue(value.Item3);
    }

    [Preserve]
    public override void Deserialize(ref MemoryPackReader reader, ref ValueTuple<T1?, T2?, T3?> value)
    {
        if (!System.Runtime.CompilerServices.RuntimeHelpers.IsReferenceOrContainsReferences<ValueTuple<T1?, T2?, T3?>>())
        {
            reader.DangerousReadUnmanaged(out value);
            return;
        }

        value = new ValueTuple<T1?, T2?, T3?>(
            reader.ReadValue<T1>(),
            reader.ReadValue<T2>(),
            reader.ReadValue<T3>()
        );
    }
}

[Preserve]
public sealed class ValueTupleFormatter<T1, T2, T3, T4> : MemoryPackFormatter<ValueTuple<T1?, T2?, T3?, T4?>>
{
    [Preserve]
    public override void Serialize(ref MemoryPackWriter writer, ref ValueTuple<T1?, T2?, T3?, T4?> value)
    {
        if (!System.Runtime.CompilerServices.RuntimeHelpers.IsReferenceOrContainsReferences<ValueTuple<T1?, T2?, T3?, T4?>>())
        {
            writer.DangerousWriteUnmanaged(value);
            return;
        }

        writer.WriteValue(value.Item1);
        writer.WriteValue(value.Item2);
        writer.WriteValue(value.Item3);
        writer.WriteValue(value.Item4);
    }

    [Preserve]
    public override void Deserialize(ref MemoryPackReader reader, ref ValueTuple<T1?, T2?, T3?, T4?> value)
    {
        if (!System.Runtime.CompilerServices.RuntimeHelpers.IsReferenceOrContainsReferences<ValueTuple<T1?, T2?, T3?, T4?>>())
        {
            reader.DangerousReadUnmanaged(out value);
            return;
        }

        value = new ValueTuple<T1?, T2?, T3?, T4?>(
            reader.ReadValue<T1>(),
            reader.ReadValue<T2>(),
            reader.ReadValue<T3>(),
            reader.ReadValue<T4>()
        );
    }
}

[Preserve]
public sealed class ValueTupleFormatter<T1, T2, T3, T4, T5> : MemoryPackFormatter<ValueTuple<T1?, T2?, T3?, T4?, T5?>>
{
    [Preserve]
    public override void Serialize(ref MemoryPackWriter writer, ref ValueTuple<T1?, T2?, T3?, T4?, T5?> value)
    {
        if (!System.Runtime.CompilerServices.RuntimeHelpers.IsReferenceOrContainsReferences<ValueTuple<T1?, T2?, T3?, T4?, T5?>>())
        {
            writer.DangerousWriteUnmanaged(value);
            return;
        }

        writer.WriteValue(value.Item1);
        writer.WriteValue(value.Item2);
        writer.WriteValue(value.Item3);
        writer.WriteValue(value.Item4);
        writer.WriteValue(value.Item5);
    }

    [Preserve]
    public override void Deserialize(ref MemoryPackReader reader, ref ValueTuple<T1?, T2?, T3?, T4?, T5?> value)
    {
        if (!System.Runtime.CompilerServices.RuntimeHelpers.IsReferenceOrContainsReferences<ValueTuple<T1?, T2?, T3?, T4?, T5?>>())
        {
            reader.DangerousReadUnmanaged(out value);
            return;
        }

        value = new ValueTuple<T1?, T2?, T3?, T4?, T5?>(
            reader.ReadValue<T1>(),
            reader.ReadValue<T2>(),
            reader.ReadValue<T3>(),
            reader.ReadValue<T4>(),
            reader.ReadValue<T5>()
        );
    }
}

[Preserve]
public sealed class ValueTupleFormatter<T1, T2, T3, T4, T5, T6> : MemoryPackFormatter<ValueTuple<T1?, T2?, T3?, T4?, T5?, T6?>>
{
    [Preserve]
    public override void Serialize(ref MemoryPackWriter writer, ref ValueTuple<T1?, T2?, T3?, T4?, T5?, T6?> value)
    {
        if (!System.Runtime.CompilerServices.RuntimeHelpers.IsReferenceOrContainsReferences<ValueTuple<T1?, T2?, T3?, T4?, T5?, T6?>>())
        {
            writer.DangerousWriteUnmanaged(value);
            return;
        }

        writer.WriteValue(value.Item1);
        writer.WriteValue(value.Item2);
        writer.WriteValue(value.Item3);
        writer.WriteValue(value.Item4);
        writer.WriteValue(value.Item5);
        writer.WriteValue(value.Item6);
    }

    [Preserve]
    public override void Deserialize(ref MemoryPackReader reader, ref ValueTuple<T1?, T2?, T3?, T4?, T5?, T6?> value)
    {
        if (!System.Runtime.CompilerServices.RuntimeHelpers.IsReferenceOrContainsReferences<ValueTuple<T1?, T2?, T3?, T4?, T5?, T6?>>())
        {
            reader.DangerousReadUnmanaged(out value);
            return;
        }

        value = new ValueTuple<T1?, T2?, T3?, T4?, T5?, T6?>(
            reader.ReadValue<T1>(),
            reader.ReadValue<T2>(),
            reader.ReadValue<T3>(),
            reader.ReadValue<T4>(),
            reader.ReadValue<T5>(),
            reader.ReadValue<T6>()
        );
    }
}

[Preserve]
public sealed class ValueTupleFormatter<T1, T2, T3, T4, T5, T6, T7> : MemoryPackFormatter<ValueTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?>>
{
    [Preserve]
    public override void Serialize(ref MemoryPackWriter writer, ref ValueTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?> value)
    {
        if (!System.Runtime.CompilerServices.RuntimeHelpers.IsReferenceOrContainsReferences<ValueTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?>>())
        {
            writer.DangerousWriteUnmanaged(value);
            return;
        }

        writer.WriteValue(value.Item1);
        writer.WriteValue(value.Item2);
        writer.WriteValue(value.Item3);
        writer.WriteValue(value.Item4);
        writer.WriteValue(value.Item5);
        writer.WriteValue(value.Item6);
        writer.WriteValue(value.Item7);
    }

    [Preserve]
    public override void Deserialize(ref MemoryPackReader reader, ref ValueTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?> value)
    {
        if (!System.Runtime.CompilerServices.RuntimeHelpers.IsReferenceOrContainsReferences<ValueTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?>>())
        {
            reader.DangerousReadUnmanaged(out value);
            return;
        }

        value = new ValueTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?>(
            reader.ReadValue<T1>(),
            reader.ReadValue<T2>(),
            reader.ReadValue<T3>(),
            reader.ReadValue<T4>(),
            reader.ReadValue<T5>(),
            reader.ReadValue<T6>(),
            reader.ReadValue<T7>()
        );
    }
}

[Preserve]
public sealed class ValueTupleFormatter<T1, T2, T3, T4, T5, T6, T7, TRest> : MemoryPackFormatter<ValueTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, TRest>>
    where TRest : struct
{
    [Preserve]
    public override void Serialize(ref MemoryPackWriter writer, ref ValueTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, TRest> value)
    {
        if (!System.Runtime.CompilerServices.RuntimeHelpers.IsReferenceOrContainsReferences<ValueTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, TRest>>())
        {
            writer.DangerousWriteUnmanaged(value);
            return;
        }

        writer.WriteValue(value.Item1);
        writer.WriteValue(value.Item2);
        writer.WriteValue(value.Item3);
        writer.WriteValue(value.Item4);
        writer.WriteValue(value.Item5);
        writer.WriteValue(value.Item6);
        writer.WriteValue(value.Item7);
        writer.WriteValue(value.Rest);
    }

    [Preserve]
    public override void Deserialize(ref MemoryPackReader reader, ref ValueTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, TRest> value)
    {
        if (!System.Runtime.CompilerServices.RuntimeHelpers.IsReferenceOrContainsReferences<ValueTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, TRest>>())
        {
            reader.DangerousReadUnmanaged(out value);
            return;
        }

        value = new ValueTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, TRest>(
            reader.ReadValue<T1>(),
            reader.ReadValue<T2>(),
            reader.ReadValue<T3>(),
            reader.ReadValue<T4>(),
            reader.ReadValue<T5>(),
            reader.ReadValue<T6>(),
            reader.ReadValue<T7>(),
            reader.ReadValue<TRest>()!
        );
    }
}


}