using System;
using System.Collections.Generic;
using Random = System.Random;

namespace ET
{
    public class RandomGenerator: Singleton<RandomGenerator>
    {
        private readonly Random random = new Random(Guid.NewGuid().GetHashCode());

        private readonly byte[] byte8 = new byte[8];

        public ulong RandUInt64()
        {
            random.NextBytes(byte8);
            return BitConverter.ToUInt64(byte8, 0);
        }

        public int RandInt32()
        {
            return random.Next();
        }

        public uint RandUInt32()
        {
            return (uint) random.Next();
        }

        public long RandInt64()
        {
            random.NextBytes(byte8);
            return BitConverter.ToInt64(byte8, 0);
        }

        /// <summary>
        /// 获取lower与Upper之间的随机数,包含下限，不包含上限
        /// </summary>
        /// <param name="lower"></param>
        /// <param name="upper"></param>
        /// <returns></returns>
        public int RandomNumber(int lower, int upper)
        {
            int value = random.Next(lower, upper);
            return value;
        }

        public long NextLong(long minValue, long maxValue)
        {
            if (minValue > maxValue)
            {
                throw new ArgumentException("minValue is great than maxValue", nameof (minValue));
            }

            long num = maxValue - minValue;
            return minValue + (long) (random.NextDouble() * num);
        }

        public bool RandomBool()
        {
            return random.Next(2) == 0;
        }

        public T RandomArray<T>(T[] array)
        {
            return array[RandomNumber(0, array.Length)];
        }

        public T RandomArray<T>(List<T> array)
        {
            return array[RandomNumber(0, array.Count)];
        }

        /// <summary>
        /// 打乱数组
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="arr">要打乱的数组</param>
        public void BreakRank<T>(List<T> arr)
        {
            if (arr == null || arr.Count < 2)
            {
                return;
            }

            for (int i = 0; i < arr.Count; i++)
            {
                int index = random.Next(0, arr.Count);
                (arr[index], arr[i]) = (arr[i], arr[index]);
            }
        }

        public float RandFloat01()
        {
            int a = RandomNumber(0, 1000000);
            return a / 1000000f;
        }
    }
}