using System;

namespace Helper
{
    public static class RandomHelper
    {
        public static UInt64 RandUInt64()
        {
            var bytes = new byte[8];
            var random = new Random();
            random.NextBytes(bytes);
            return BitConverter.ToUInt64(bytes, 0);
        }
    }
}