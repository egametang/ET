using System;

namespace DotRecast.Core
{
    public static class RcArrayUtils
    {
        public static T[] CopyOf<T>(T[] source, int startIdx, int length)
        {
            var deatArr = new T[length];
            for (int i = 0; i < length; ++i)
            {
                deatArr[i] = source[startIdx + i];
            }

            return deatArr;
        }

        public static T[] CopyOf<T>(T[] source, int length)
        {
            var deatArr = new T[length];
            var count = Math.Max(0, Math.Min(source.Length, length));
            for (int i = 0; i < count; ++i)
            {
                deatArr[i] = source[i];
            }

            return deatArr;
        }

        public static T[][] Of<T>(int len1, int len2)
        {
            var temp = new T[len1][];

            for (int i = 0; i < len1; ++i)
            {
                temp[i] = new T[len2];
            }

            return temp;
        }
    }
}