using System.Collections.Generic;
using System.Text;

namespace Bright.Common
{
    public static class StringUtil
    {
        public static string ToStr(object o)
        {
            return ToStr(o, new StringBuilder());
        }

        public static string ToStr(object o, StringBuilder sb)
        {
            foreach (var p in o.GetType().GetFields())
            {

                sb.Append($"{p.Name} = {p.GetValue(o)},");
            }

            foreach (var p in o.GetType().GetProperties())
            {
                sb.Append($"{p.Name} = {p.GetValue(o)},");
            }
            return sb.ToString();
        }

        public static string ArrayToString<T>(T[] arr)
        {
            return "[" + string.Join(",", arr) + "]";
        }


        public static string CollectionToString<T>(IEnumerable<T> arr)
        {
            return "[" + string.Join(",", arr) + "]";
        }


        public static string CollectionToString<TK, TV>(IDictionary<TK, TV> dic)
        {
            var sb = new StringBuilder('{');
            foreach (var e in dic)
            {
                sb.Append(e.Key).Append(':');
                sb.Append(e.Value).Append(',');
            }
            sb.Append('}');
            return sb.ToString();
        }
    }
}
