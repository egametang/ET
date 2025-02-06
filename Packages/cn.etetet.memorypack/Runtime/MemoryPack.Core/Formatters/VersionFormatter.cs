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

[Preserve]
public sealed class VersionFormatter : MemoryPackFormatter<Version>
{
    // Serialize as [Major, Minor, Build, Revision]

    [Preserve]
    public override void Serialize(ref MemoryPackWriter writer, ref Version? value)
    {
        if (value == null)
        {
            writer.WriteNullObjectHeader();
            return;
        }

        writer.WriteUnmanagedWithObjectHeader(4, value.Major, value.Minor, value.Build, value.Revision);
    }

    [Preserve]
    public override void Deserialize(ref MemoryPackReader reader, ref Version? value)
    {
        if (!reader.TryReadObjectHeader(out var count))
        {
            value = null;
            return;
        }

        if (count != 4) MemoryPackSerializationException.ThrowInvalidPropertyCount(4, count);

        reader.ReadUnmanaged(out int major, out int minor, out int build, out int revision);

        // when use new Version(major, minor), build and revision will be -1, it can not use constructor.
        if (revision == -1)
        {
            if (build == -1)
            {
                value = new Version(major, minor);
            }
            else
            {
                value = new Version(major, minor, build);
            }
        }
        else
        {
            value = new Version(major, minor, build, revision);
        }
    }
}

}