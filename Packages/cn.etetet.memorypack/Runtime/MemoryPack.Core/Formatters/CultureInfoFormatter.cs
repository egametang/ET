using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

#nullable enable
using System.Globalization;
using MemoryPack.Internal;

namespace MemoryPack.Formatters {

[Preserve]
public sealed class CultureInfoFormatter : MemoryPackFormatter<CultureInfo>
{
    // treat as a string(Name).

    [Preserve]
    public override void Serialize(ref MemoryPackWriter writer, ref CultureInfo? value)
    {
        writer.WriteString(value?.Name);
    }

    [Preserve]
    public override void Deserialize(ref MemoryPackReader reader, ref CultureInfo? value)
    {
        var str = reader.ReadString();
        if (str == null)
        {
            value = null;
        }
        else
        {
            value = CultureInfo.GetCultureInfo(str);
        }
    }
}
}