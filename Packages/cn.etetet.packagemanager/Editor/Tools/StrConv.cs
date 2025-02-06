using System;
using UnityEngine;

namespace ET.PackageManager.Editor
{
    /// <summary>
    /// 处理字符串转换的类
    /// </summary>
    public static class StrConv
    {
        #region 数组分隔符

        public const string ArrSplitLv1 = "|";
        public const string ArrSplitLv2 = "*";
        public const string ArrSplitLv3 = "$";
        public const string ArrSplitLv4 = "#";

        public const char ChrArrSplitLv1 = '|';
        public const char ChrArrSplitLv2 = '*';
        public const char ChrArrSplitLv3 = '$';
        public const char ChrArrSplitLv4 = '#';

        #endregion 数组分隔符

        /// <summary>
        /// 从字符串转值到u16
        /// </summary>
        /// <param name="valueIn"></param>
        /// <param name="valueOut"></param>
        /// <returns></returns>
        public static bool ToU16(string valueIn, out ushort valueOut)
        {
            if (string.IsNullOrEmpty(valueIn))
            {
                valueOut = 0;
                return false;
            }

            bool success = ushort.TryParse(valueIn, out valueOut);
            if (!success)
            {
                Debug.LogError("UInt16.TryParse error");
                valueOut = 0;
                return false;
            }

            return true;
        }

        /// <summary>
        /// 从字符串转值到u32
        /// </summary>
        /// <param name="valueIn"></param>
        /// <param name="valueOut"></param>
        /// <returns></returns>
        public static bool ToU32(string valueIn, out uint valueOut)
        {
            if (string.IsNullOrEmpty(valueIn))
            {
                valueOut = 0;
                return false;
            }

            bool success = uint.TryParse(valueIn, out valueOut);
            if (!success)
            {
                Debug.LogError("UInt32.TryParse error");
                valueOut = 0;
                return false;
            }

            return true;
        }

        /// <summary>
        /// 从字符串转值到u32，直接返回值
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static uint ToU32(string value)
        {
            uint.TryParse(value, out uint newValue);
            return newValue;
        }

        /// <summary>
        /// 从字符串转值到u64，直接返回值
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static ulong ToU64(string value)
        {
            ulong.TryParse(value, out ulong newValue);
            return newValue;
        }

        /// <summary>
        /// 从字符串转值到i32
        /// </summary>
        /// <param name="valueIn"></param>
        /// <param name="valueOut"></param>
        /// <returns></returns>
        public static bool ToI32(string valueIn, out int valueOut)
        {
            if (string.IsNullOrEmpty(valueIn))
            {
                valueOut = 0;
                return false;
            }

            bool success = Int32.TryParse(valueIn, out valueOut);
            if (!success)
            {
                Debug.LogError("UInt32.TryParse error");
                valueOut = 0;
                return false;
            }

            return true;
        }

        /// <summary>
        /// 从字符串转值到i32， 直接返回值
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int ToI32(string value)
        {
            Int32.TryParse(value, out int newValue);
            return newValue;
        }

        /// <summary>
        /// 从字符串转值到float
        /// </summary>
        /// <param name="valueIn"></param>
        /// <param name="valueOut"></param>
        /// <param name="per">默认值为1</param>
        /// <returns></returns>
        public static bool ToFloat(string valueIn, out float valueOut, float per)
        {
            if (string.IsNullOrEmpty(valueIn))
            {
                valueOut = 0;
                return false;
            }

            bool success = float.TryParse(valueIn, out valueOut);
            if (!success)
            {
                Debug.LogError("float.TryParse error");
                valueOut = 0;
                return false;
            }

            valueOut = valueOut * per;
            return true;
        }

        /// <summary>
        /// 解析千分比
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static float ToPer1000(string value)
        {
            uint v2 = ToU32(value);
            return v2 * 0.001f;
        }

        /// <summary>
        /// 从字符串转值到Byte
        /// </summary>
        /// <param name="valueIn"></param>
        /// <param name="valueOut"></param>
        /// <returns></returns>
        public static bool ToByte(string valueIn, out byte valueOut)
        {
            if (string.IsNullOrEmpty(valueIn))
            {
                valueOut = 0;
                return false;
            }

            bool success = byte.TryParse(valueIn, out valueOut);
            if (!success)
            {
                Debug.LogError("Byte.TryParse error");
                valueOut = 0;
                return false;
            }

            return true;
        }

        /// <summary>
        /// 将文本转为数值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="input"></param>
        /// <param name="defValue"></param>
        /// <returns></returns>
        public static T ToNumber<T>(string input, T defValue = default)
        {
            return ToNumber(input, out T value) ? value : defValue;
        }

        /// <summary>
        /// 通用的将文本转为数值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="input"></param>
        /// <param name="output"></param>
        /// <returns></returns>
        public static bool ToNumber<T>(string input, out T output)
        {
            try
            {
                object t;
                switch (typeof(T).GetNumericType())
                {
                    case NumericType.Int32:
                        int.TryParse(input, out var valueInt32);
                        t = valueInt32;
                        break;
                    case NumericType.UInt32:
                        uint.TryParse(input, out var valueUInt32);
                        t = valueUInt32;
                        break;
                    case NumericType.Float:
                        float.TryParse(input, out var valueFloat);
                        t = valueFloat;
                        break;
                    case NumericType.Int16:
                        Int16.TryParse(input, out var valueInt16);
                        t = valueInt16;
                        break;
                    case NumericType.Int64:
                        Int64.TryParse(input, out var valueInt64);
                        t = valueInt64;
                        break;
                    case NumericType.UInt16:
                        UInt16.TryParse(input, out var valueUInt16);
                        t = valueUInt16;
                        break;
                    case NumericType.UInt64:
                        UInt64.TryParse(input, out var valueUInt64);
                        t = valueUInt64;
                        break;
                    case NumericType.Byte:
                        byte.TryParse(input, out var valueByte);
                        t = valueByte;
                        break;
                    case NumericType.SByte:
                        sbyte.TryParse(input, out var valueSByte);
                        t = valueSByte;
                        break;
                    case NumericType.Double:
                        double.TryParse(input, out var valueDouble);
                        t = valueDouble;
                        break;
                    case NumericType.Decimal:
                        decimal.TryParse(input, out var valueDecimal);
                        t = valueDecimal;
                        break;
                    default:
                        output = default;
                        return false;
                }

                output = (T)t;
                return true;
            }
            catch (Exception)
            {
                output = default;
                return false;
            }
        }

        /// <summary>
        /// 从字符串转值到enum
        /// </summary>
        /// <typeparam name="TEnum"></typeparam>
        /// <param name="valueIn"></param>
        /// <param name="valueOut"></param>
        /// <returns></returns>
        public static bool ToEnum<TEnum>(string valueIn, out TEnum valueOut)
        {
            valueOut = (TEnum)Enum.Parse(typeof(TEnum), valueIn);
            return true;
        }

        /// <summary>
        /// 从字符串转值到enum
        /// </summary>
        /// <typeparam name="TEnum"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static TEnum ToEnum<TEnum>(string value)
        {
            return (TEnum)Enum.Parse(typeof(TEnum), value);
        }

        /// <summary>
        /// 通过一定规则，把字符串转为数组
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="valueIn"></param>
        /// <param name="separator"></param>
        /// <param name="valueOut"></param>
        /// <param name="parse">解析器</param>
        /// <returns></returns>
        public static bool ToArr<T>(string valueIn, string separator, out T[] valueOut, Func<string, T> parse)
        {
            if (string.IsNullOrEmpty(valueIn))
            {
                valueOut = new T[0];
                return true;
            }

            string[] strValueArr = valueIn.Split(new[] { separator }, StringSplitOptions.None);

            Type tType = typeof(T);
            if (typeof(string) == tType)
            {
                valueOut = strValueArr as T[];
                return true;
            }

            if (parse == null)
            {
                throw new NullReferenceException("如果项非string类型，则必需设置解析器");
            }

            valueOut = new T[strValueArr.Length];
            for (int i = 0; i < strValueArr.Length; i++)
            {
                valueOut[i] = parse(strValueArr[i]);
            }

            return true;
        }

        /// <summary>
        /// 通过一定规则，把字答串数组转为对应类型数组
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="values"></param>
        /// <param name="parse"></param>
        /// <returns></returns>
        public static T[] ToArr<T>(string[] values, Func<string, T> parse)
        {
            T[] valueOut = new T[values.Length];
            for (int i = 0; i < values.Length; i++)
            {
                valueOut[i] = parse(values[i]);
            }

            return valueOut;
        }

        public static T[] ToArr<T>(string value, string separator = ArrSplitLv1)
        {
            if (ToArr(value, separator, out T[] item, ParseAny<T>))
            {
                return item;
            }

            return new T[0];
        }

        /// <summary>
        /// 主要用在ParseArr
        /// 支持string, enum, number的解析
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static T ParseAny<T>(string value)
        {
            Type tType = typeof(T);
            if (tType.IsEnum)
            {
                return ToEnum<T>(value);
            }

            if (tType == typeof(string))
            {
                return (T)(object)value;
            }

            if (!double.TryParse(value, out double doubleValue))
            {
                return default;
            }

            if (typeof(T) == typeof(double))
            {
                return (T)(object)doubleValue;
            }

            try
            {
                object o = Convert.ChangeType(doubleValue, typeof(T));
                return (T)o;
            }
            catch (Exception e)
            {
                Debug.LogError($"Error:{e.Message}{e.StackTrace}");
                return default;
            }
        }
    }
}
