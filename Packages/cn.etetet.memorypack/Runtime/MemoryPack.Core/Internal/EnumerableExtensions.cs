using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

#nullable enable
namespace MemoryPack.Internal {

internal static class EnumerableExtensions
{
    public static bool TryGetNonEnumeratedCountEx<T>(this IEnumerable<T> value, out int count)
    {
        // TryGetNonEnumeratedCount is not check IReadOnlyCollection<T> so add check manually.
        // https://github.com/dotnet/runtime/issues/54764

#if NET7_0_OR_GREATER
        if (value.TryGetNonEnumeratedCount(out count))
        {
            return true;
        }
#else
        count = 0;
        if (value is ICollection<T> collection)
        {
            count = collection.Count;
            return true;
        }
#endif

        if (value is IReadOnlyCollection<T> readOnlyCollection)
        {
            count = readOnlyCollection.Count;
            return true;
        }

        return false;
    }
}

}