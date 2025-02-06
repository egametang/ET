using System.Collections.Generic;
using System.Text;

namespace ET.PackageManager.Editor
{
    /// <summary>
    /// StringBuilder池
    /// </summary>
    public static class SbPool
    {
        private static Stack<StringBuilder> g_pool = new Stack<StringBuilder>(5);

        public static StringBuilder Get()
        {
            lock (g_pool)
            {
                return g_pool.Count == 0
                        ? new StringBuilder()
                        : g_pool.Pop();
            }
        }

        public static void Put(StringBuilder sb)
        {
            sb.Clear();
            lock (g_pool)
            {
                g_pool.Push(sb);
            }
        }

        public static string PutAndToStr(StringBuilder sb)
        {
            var str = sb.ToString();
            Put(sb);
            return str;
        }
    }
}
