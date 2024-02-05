using System;
using MemoryPack;
using MongoDB.Bson.Serialization.Attributes;

namespace TrueSync {

    /**
     *  @brief Generates random numbers based on a deterministic approach.
     **/
    [MemoryPackable]
    public partial class TSRandom {
        // From http://www.codeproject.com/Articles/164087/Random-Number-Generation
        // Class TSRandom generates random numbers
        // from a uniform distribution using the Mersenne
        // Twister algorithm.
        private const int N = 624;
        private const int M = 397;
        private const uint MATRIX_A = 0x9908b0dfU;
        private const uint UPPER_MASK = 0x80000000U;
        private const uint LOWER_MASK = 0x7fffffffU;
        private const int MAX_RAND_INT = 0x7fffffff;
        [BsonElement]
        [MemoryPackInclude]
        private uint[] mag01 = { 0x0U, MATRIX_A };
        [BsonElement]
        [MemoryPackInclude]
        private uint[] mt = new uint[N];
        [BsonElement]
        [MemoryPackInclude]
        private int mti = N + 1;

        [MemoryPackConstructor]
        private TSRandom() {
        }

        private TSRandom(int seed) {
            init_genrand((uint)seed);
        }

        private TSRandom(int[] init) {
            uint[] initArray = new uint[init.Length];
            for (int i = 0; i < init.Length; ++i)
                initArray[i] = (uint)init[i];
            init_by_array(initArray, (uint)initArray.Length);
        }

        public static int MaxRandomInt { get { return 0x7fffffff; } }

        /**
         *  @brief Returns a random integer.
         **/
        public int Next() {
            return genrand_int31();
        }

        /**
         *  @brief Returns a random integer.
         **/
        public int CallNext() {
            return this.Next();
        }

        /**
         *  @brief Returns a integer between a min value [inclusive] and a max value [exclusive].
         **/
        public int Next(int minValue, int maxValue) {
            if (minValue > maxValue) {
                int tmp = maxValue;
                maxValue = minValue;
                minValue = tmp;
            }

            int range = maxValue - minValue;

            return minValue + Next() % range;
        }

        /**
         *  @brief Returns a {@link FP} between a min value [inclusive] and a max value [inclusive].
         **/
        public FP Next(float minValue, float maxValue) {
            int minValueInt = (int)(minValue * 1000), maxValueInt = (int)(maxValue * 1000);

            if (minValueInt > maxValueInt) {
                int tmp = maxValueInt;
                maxValueInt = minValueInt;
                minValueInt = tmp;
            }

            return (FP.Floor((maxValueInt - minValueInt + 1) * NextFP() +
                minValueInt)) / 1000;
        }

        /**
         *  @brief Returns a integer between a min value [inclusive] and a max value [exclusive].
         **/
        public int Range(int minValue, int maxValue) {
            return this.Next(minValue, maxValue);
        }

        /**
         *  @brief Returns a {@link FP} between a min value [inclusive] and a max value [inclusive].
         **/
        public FP Range(float minValue, float maxValue) {
            return this.Next(minValue, maxValue);
        }

        /**
         *  @brief Returns a {@link FP} between 0.0 [inclusive] and 1.0 [inclusive].
         **/
        public FP NextFP() {
            return ((FP) Next()) / (MaxRandomInt);
        }

        /**
         *  @brief Returns a {@link FP} between 0.0 [inclusive] and 1.0 [inclusive].
         **/
        [MemoryPackIgnore]
        public FP value {
            get {
                return this.NextFP();
            }
        }

        /**
         *  @brief Returns a random {@link TSVector} representing a point inside a sphere with radius 1.
         **/
        [MemoryPackIgnore]
        public TSVector insideUnitSphere {
            get {
                return new TSVector(value, value, value);
            }
        }

        private float NextFloat() {
            return (float)genrand_real2();
        }

        private float NextFloat(bool includeOne) {
            if (includeOne) {
                return (float)genrand_real1();
            }
            return (float)genrand_real2();
        }

        private float NextFloatPositive() {
            return (float)genrand_real3();
        }

        private double NextDouble() {
            return genrand_real2();
        }

        private double NextDouble(bool includeOne) {
            if (includeOne) {
                return genrand_real1();
            }
            return genrand_real2();
        }

        private double NextDoublePositive() {
            return genrand_real3();
        }

        private double Next53BitRes() {
            return genrand_res53();
        }

        public void Initialize() {
            init_genrand((uint)DateTime.Now.Millisecond);
        }

        public void Initialize(int seed) {
            init_genrand((uint)seed);
        }

        public void Initialize(int[] init) {
            uint[] initArray = new uint[init.Length];
            for (int i = 0; i < init.Length; ++i)
                initArray[i] = (uint)init[i];
            init_by_array(initArray, (uint)initArray.Length);
        }

        private void init_genrand(uint s) {
            mt[0] = s & 0xffffffffU;
            for (mti = 1; mti < N; mti++) {
                mt[mti] = (uint)(1812433253U * (mt[mti - 1] ^ (mt[mti - 1] >> 30)) + mti);
                mt[mti] &= 0xffffffffU;
            }
        }

        private void init_by_array(uint[] init_key, uint key_length) {
            int i, j, k;
            init_genrand(19650218U);
            i = 1;
            j = 0;
            k = (int)(N > key_length ? N : key_length);
            for (; k > 0; k--) {
                mt[i] = (uint)((uint)(mt[i] ^ ((mt[i - 1] ^ (mt[i - 1] >> 30)) * 1664525U)) + init_key[j] + j);
                mt[i] &= 0xffffffffU;
                i++;
                j++;
                if (i >= N) {
                    mt[0] = mt[N - 1];
                    i = 1;
                }
                if (j >= key_length)
                    j = 0;
            }
            for (k = N - 1; k > 0; k--) {
                mt[i] = (uint)((uint)(mt[i] ^ ((mt[i - 1] ^ (mt[i - 1] >> 30)) *
                    1566083941U)) - i);
                mt[i] &= 0xffffffffU;
                i++;
                if (i >= N) {
                    mt[0] = mt[N - 1];
                    i = 1;
                }
            }
            mt[0] = 0x80000000U;
        }

        uint genrand_int32() {
            uint y;
            if (mti >= N) {
                int kk;
                if (mti == N + 1)
                    init_genrand(5489U);
                for (kk = 0; kk < N - M; kk++) {
                    y = (mt[kk] & UPPER_MASK) | (mt[kk + 1] & LOWER_MASK);
                    mt[kk] = mt[kk + M] ^ (y >> 1) ^ mag01[y & 0x1U];
                }
                for (; kk < N - 1; kk++) {
                    y = (mt[kk] & UPPER_MASK) | (mt[kk + 1] & LOWER_MASK);
                    mt[kk] = mt[kk + (M - N)] ^ (y >> 1) ^ mag01[y & 0x1U];
                }
                y = (mt[N - 1] & UPPER_MASK) | (mt[0] & LOWER_MASK);
                mt[N - 1] = mt[M - 1] ^ (y >> 1) ^ mag01[y & 0x1U];
                mti = 0;
            }
            y = mt[mti++];
            y ^= (y >> 11);
            y ^= (y << 7) & 0x9d2c5680U;
            y ^= (y << 15) & 0xefc60000U;
            y ^= (y >> 18);
            return y;
        }

        private int genrand_int31() {
            return (int)(genrand_int32() >> 1);
        }

        FP genrand_FP() {
            return (FP)genrand_int32() * (FP.One / (FP)4294967295);
        }

        double genrand_real1() {
            return genrand_int32() * (1.0 / 4294967295.0);
        }
        double genrand_real2() {
            return genrand_int32() * (1.0 / 4294967296.0);
        }

        double genrand_real3() {
            return (((double)genrand_int32()) + 0.5) * (1.0 / 4294967296.0);
        }

        double genrand_res53() {
            uint a = genrand_int32() >> 5, b = genrand_int32() >> 6;
            return (a * 67108864.0 + b) * (1.0 / 9007199254740992.0);
        }
    }

}