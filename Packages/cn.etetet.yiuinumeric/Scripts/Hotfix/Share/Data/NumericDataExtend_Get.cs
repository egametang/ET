#if UNITY_EDITOR
#define NUMERIC_CHECK_SYMBOLS //数值检查宏 离开Unity要使用就在unity设置中添加这个宏
#endif
using System.Collections.Generic;

namespace ET
{
    /// <summary>
    /// 获取
    /// </summary>
    [FriendOf(typeof(NumericDataComponent))]
    public static partial class NumericDataExtend
    {
        #region Get

        public static bool GetAsBool(this NumericData self, int numericType)
        {
            #if NUMERIC_CHECK_SYMBOLS
            numericType.CheckGetNumeric(ENumericValueType.Bool);
            #endif
            return self.GetByKey(numericType) != 0;
        }

        public static float GetAsFloat(this NumericData self, int numericType)
        {
            #if NUMERIC_CHECK_SYMBOLS
            numericType.CheckGetNumeric(ENumericValueType.Float);
            #endif
            return self.GetByKey(numericType) / NumericConst.FloatRate;
        }

        public static int GetAsInt(this NumericData self, int numericType)
        {
            #if NUMERIC_CHECK_SYMBOLS
            numericType.CheckGetNumeric(ENumericValueType.Int);
            #endif
            return (int)self.GetByKey(numericType);
        }

        public static long GetAsLong(this NumericData self, int numericType)
        {
            #if NUMERIC_CHECK_SYMBOLS
            numericType.CheckGetNumeric(ENumericValueType.Long);
            #endif
            return self.GetByKey(numericType);
        }

        public static bool GetAsBool(this NumericData self, ENumericType numericType)
        {
            #if NUMERIC_CHECK_SYMBOLS
            numericType.CheckGetNumeric(ENumericValueType.Bool);
            #endif
            return self.GetByKey((int)numericType) != 0;
        }

        public static float GetAsFloat(this NumericData self, ENumericType numericType)
        {
            #if NUMERIC_CHECK_SYMBOLS
            numericType.CheckGetNumeric(ENumericValueType.Float);
            #endif
            return self.GetByKey((int)numericType) / NumericConst.FloatRate;
        }

        public static int GetAsInt(this NumericData self, ENumericType numericType)
        {
            #if NUMERIC_CHECK_SYMBOLS
            numericType.CheckGetNumeric(ENumericValueType.Int);
            #endif
            return (int)self.GetByKey((int)numericType);
        }

        public static long GetAsLong(this NumericData self, ENumericType numericType)
        {
            #if NUMERIC_CHECK_SYMBOLS
            numericType.CheckGetNumeric(ENumericValueType.Long);
            #endif
            return self.GetByKey((int)numericType);
        }

        #region Other

        //获取obj类型的值 特殊情况下使用 一般不用这个方法
        public static object GetObjectValue(this NumericData self, ENumericType numeric)
        {
            //从检查表判断数值类型
            var checkCfg = numeric.GetCheckConfig();
            if (checkCfg == null) return 0;

            object value;

            //根据数值类型来调用不同的获取方法
            switch (checkCfg.Check)
            {
                case ENumericValueType.Int:
                    value = self.GetAsInt(numeric);
                    break;
                case ENumericValueType.Long:
                    value = self.GetAsLong(numeric);
                    break;
                case ENumericValueType.Bool:
                    //这个类型一般不会面向玩家 所以自己看看就行了
                    //如果有这个需求 在这里进行扩展
                    var boolValue = self.GetAsBool(numeric);
                    return boolValue ? "True" : "False";
                case ENumericValueType.Float:
                    value = self.GetAsFloat(numeric);
                    break;
                default:
                    Log.Error($"类型错误请检查 {checkCfg.Check}");
                    return 0;
            }

            return value;
        }

        //只获取字典的数据
        public static Dictionary<int, long> GetNumericDic(this NumericData self)
        {
            var refDic = new Dictionary<int, long>();
            self.GetNumericDic(refDic);
            return refDic;
        }

        //只获取字典的数据
        //如果你想什么多个数值相加什么的 应该考虑使用copy方法
        //这里更多是给服务器传送时使用
        public static void GetNumericDic(this NumericData self, Dictionary<int, long> refDic)
        {
            refDic.Clear();
            foreach ((int key, long value) in self.NumericDic)
            {
                refDic.Add(key, value);
            }
        }

        public static void GetNumericDic(this NumericData self, NumericDictionaryPool<int, long> refDic)
        {
            refDic.Clear();
            foreach ((int key, long value) in self.NumericDic)
            {
                refDic.Add(key, value);
            }
        }

        #endregion

        #endregion
    }
}
