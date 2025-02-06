using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

#nullable enable
using MemoryPack.Internal;
using System.Buffers;

namespace MemoryPack.Formatters {

[Preserve]
public sealed class TimeZoneInfoFormatter : MemoryPackFormatter<TimeZoneInfo>
{
    [Preserve]
    public override void Serialize(ref MemoryPackWriter writer, ref TimeZoneInfo? value)
    {
        writer.WriteString(value?.ToSerializedString());
    }

    [Preserve]
    public override void Deserialize(ref MemoryPackReader reader, ref TimeZoneInfo? value)
    {
        var source = reader.ReadString();
        if (source == null)
        {
            value = null;
            return;
        }

        value = TimeZoneInfo.FromSerializedString(source);
    }
}

}