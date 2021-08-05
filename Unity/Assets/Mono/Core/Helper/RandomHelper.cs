using System;
using System.Collections;
using System.Collections.Generic;
using Random = System.Random;

namespace ET
{
    public static class RandomEx
    {
        public static ulong RandUInt64(this Random random)
        {
            byte[] byte8 = new byte[8];
            random.NextBytes(byte8);
            return BitConverter.ToUInt64(byte8, 0);
        }

        public static int RandInt32(this Random random)
        {
            return random.Next();
        }
        
        public static uint RandUInt32(this Random random)
        {
            return (uint)random.Next();
        }

        public static long RandInt64(this Random random)
        {
            byte[] byte8 = new byte[8];
            random.NextBytes(byte8);
            return BitConverter.ToInt64(byte8, 0);
        }
    }
    
    public static class RandomHelper
    {
        public static Random random = new Random(Guid.NewGuid().GetHashCode());
        
        public static ulong RandUInt64()
        {
            byte[] byte8 = new byte[8];
            random.NextBytes(byte8);
            return BitConverter.ToUInt64(byte8, 0);
        }

        public static int RandInt32()
        {
            return random.Next();
        }
        
        public static uint RandUInt32()
        {
            return (uint)random.Next();
        }

        public static long RandInt64()
        {
            byte[] byte8 = new byte[8];
            random.NextBytes(byte8);
            return BitConverter.ToInt64(byte8, 0);
        }

        /// <summary>
        /// 获取lower与Upper之间的随机数,包含下限，不包含上限
        /// </summary>
        /// <param name="lower"></param>
        /// <param name="upper"></param>
        /// <returns></returns>
        public static int RandomNumber(int lower, int upper)
        {
            int value = random.Next(lower, upper);
            return value;
        }

        public static long NextLong(long minValue,long maxValue)
        {
            if (minValue > maxValue)
            {
                throw new ArgumentException("minValue is great than maxValue", "minValue");
            }
            
            long num = maxValue - minValue;
            return minValue + (long) (random.NextDouble() * num);
        }

        public static bool RandomBool()
        {
            return random.Next(2) == 0;
        }

        public static T RandomArray<T>(this T[] array)
        {
            return array[RandomNumber(0, array.Length)];
        }

        public static int RandomArray_Len2(this int[] array)
        {
            return RandomHelper.RandomNumber(array[0], array[1]);
        }

        public static T RandomArray<T>(this List<T> array)
        {
            return array[RandomNumber(0, array.Count)];
        }

        /// <summary>
        /// 打乱数组
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="arr">要打乱的数组</param>
        public static void BreakRank<T>(this List<T> arr)
        {
            if (arr == null || arr.Count < 2)
            {
                return;
            }

            for (int i = 0; i < arr.Count; i++)
            {
                int index = random.Next(0, arr.Count);
                T temp = arr[index];
                arr[index] = arr[i];
                arr[i] = temp;
            }
        }

        public static int[] GetRandoms(int sum, int min, int max)
        {
            int[] arr = new int[sum];
            int j = 0;
            //表示键和值对的集合。
            Hashtable hashtable = new Hashtable();
            Random rm = random;
            while (hashtable.Count < sum) {
                //返回一个min到max之间的随机数
                int nValue = rm.Next(min, max);
                // 是否包含特定值
                if (!hashtable.ContainsValue(nValue))
                {
                    //把键和值添加到hashtable
                    hashtable.Add(nValue, nValue);
                    arr[j] = nValue;
                    j++;
                }
            }

            return arr;
        }
        
        /// <summary>
        /// 随机从数组中取若干个不重复的元素，
        /// 为了降低算法复杂度，所以是伪随机，对随机要求不是非常高的逻辑可以用
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sourceList"></param>
        /// <param name="destList"></param>
        /// <param name="randCount"></param>
        public static bool GetRandListByCount<T>(List<T> sourceList, List<T> destList, int randCount)
        {
            if (sourceList == null || destList == null || randCount < 0)
            {
                return false;
            }
            
            destList.Clear();

            if (randCount >= sourceList.Count)
            {
                foreach (var val in sourceList)
                {
                    destList.Add(val);
                }
                
                return true;
            }

            if (randCount == 0)
            {
                return true;
            }
            int beginIndex = random.Next(0, sourceList.Count - 1);
            for (int i = beginIndex; i < beginIndex + randCount; i++)
            {
                destList.Add(sourceList[i % sourceList.Count]);
            }

            return true;
        }

        public static float RandFloat01()
        {
            int a = RandomNumber(0, 1000000);
            return a / 1000000f;
        }

        private static int Rand(int n)
        {
            // 注意，返回值是左闭右开，所以maxValue要加1
            return random.Next(1, n + 1);
        }

        /// <summary>
        /// 通过权重随机
        /// </summary>
        /// <param name="weights"></param>
        /// <returns></returns>
        public static int RandomByWeight(int[] weights)
        {
            int sum = 0;
            for (int i = 0; i < weights.Length; i++)
            {
                sum += weights[i];
            }

            int number_rand = Rand(sum);

            int sum_temp = 0;

            for (int i = 0; i < weights.Length; i++)
            {
                sum_temp += weights[i];
                if (number_rand <= sum_temp)
                {
                    return i;
                }
            }

            return -1;
        }

        public static int RandomByWeight(List<int> weights)
        {
            if (weights.Count == 0)
            {
                return -1;
            }

            if (weights.Count == 1)
            {
                return 0;
            }

            int sum = 0;
            for (int i = 0; i < weights.Count; i++)
            {
                sum += weights[i];
            }

            int number_rand = Rand(sum);

            int sum_temp = 0;

            for (int i = 0; i < weights.Count; i++)
            {
                sum_temp += weights[i];
                if (number_rand <= sum_temp)
                {
                    return i;
                }
            }

            return -1;
        }
        
        public static int RandomByWeight(List<int> weights, int weightRandomMinVal)
        {
            if (weights.Count == 0)
            {
                return -1;
            }

            if (weights.Count == 1)
            {
                return 0;
            }

            int sum = 0;
            for (int i = 0; i < weights.Count; i++)
            {
                sum += weights[i];
            }

            int number_rand = Rand(Math.Max(sum, weightRandomMinVal));

            int sum_temp = 0;

            for (int i = 0; i < weights.Count; i++)
            {
                sum_temp += weights[i];
                if (number_rand <= sum_temp)
                {
                    return i;
                }
            }

            return -1;
        }
        
        public static int RandomByWeight(List<long> weights)
        {
            if (weights.Count == 0)
            {
                return -1;
            }

            if (weights.Count == 1)
            {
                return 0;
            }

            long sum = 0;
            for (int i = 0; i < weights.Count; i++)
            {
                sum += weights[i];
            }
            
            long number_rand = NextLong(1, sum + 1);

            long sum_temp = 0;

            for (int i = 0; i < weights.Count; i++)
            {
                sum_temp += weights[i];
                if (number_rand <= sum_temp)
                {
                    return i;
                }
            }

            return -1;
        }
    }
}