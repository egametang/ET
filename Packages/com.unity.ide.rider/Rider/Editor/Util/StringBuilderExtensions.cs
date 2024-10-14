using System.Collections.Generic;
using System.Text;

namespace Packages.Rider.Editor.Util
{
  internal static class StringBuilderExtensions
  {
    // StringBuilder.AppendJoin is very useful, but not available in 2019.2
    // It requires netstandard 2.1
    public static StringBuilder CompatibleAppendJoin(this StringBuilder stringBuilder, char separator, IEnumerable<string> parts)
    {
      var first = true;
      foreach (var part in parts)
      {
        if (!first) stringBuilder.Append(separator);
        stringBuilder.Append(part);
        first = false;
      }

      return stringBuilder;
    }
  }
}
