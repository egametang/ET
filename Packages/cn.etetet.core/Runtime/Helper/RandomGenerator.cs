using System;
using System.Collections.Generic;
using Random = System.Random;

namespace ET
{
    // 支持多线程
    public static class RandomGenerator
    {
        [StaticField]
        [ThreadStatic]
        private static Random random;

        private static Random GetRandom()
        {
            return random ??= new Random(Guid.NewGuid().GetHashCode() ^ Environment.TickCount);
        }

        public static ulong RandUInt64()
        {
            int r1 = RandInt32();
            int r2 = RandInt32();
            return ((ulong)r1 << 32) | (uint)r2;
        }

        public static int RandInt32()
        {
            return GetRandom().Next();
        }

        public static uint RandUInt32()
        {
            return (uint) GetRandom().Next();
        }

        public static long RandInt64()
        {
            uint r1 = RandUInt32();
            uint r2 = RandUInt32();
            return (long)(((ulong)r1 << 32) | r2);
        }

        /// <summary>
        /// 获取lower与Upper之间的随机数,包含下限，不包含上限
        /// </summary>
        /// <param name="lower"></param>
        /// <param name="upper"></param>
        /// <returns></returns>
        public static int RandomNumber(int lower, int upper)
        {
            int value = GetRandom().Next(lower, upper);
            return value;
        }

        public static bool RandomBool()
        {
            return GetRandom().Next(2) == 0;
        }

        public static T RandomArray<T>(T[] array)
        {
            return array[RandomNumber(0, array.Length)];
        }

        public static T RandomArray<T>(List<T> array)
        {
            return array[RandomNumber(0, array.Count)];
        }

        /// <summary>
        /// 打乱数组
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="arr">要打乱的数组</param>
        public static void BreakRank<T>(List<T> arr)
        {
            if (arr == null || arr.Count < 2)
            {
                return;
            }

            for (int i = 0; i < arr.Count; i++)
            {
                int index = GetRandom().Next(0, arr.Count);
                (arr[index], arr[i]) = (arr[i], arr[index]);
            }
        }

        public static float RandFloat01()
        {
            int a = RandomNumber(0, 1000000);
            return a / 1000000f;
        }
    }
}