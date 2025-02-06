using System;
using System.Collections.Generic;
using System.Reflection;

namespace ET.PackageManager.Editor
{
    public static class TypeExt
    {
        private static readonly Dictionary<Type, NumericType> g_numTypeMap;

        static TypeExt()
        {
            var numTypeMap = new Dictionary<Type, NumericType>(11);
            numTypeMap[typeof(byte)]    = NumericType.Byte;
            numTypeMap[typeof(sbyte)]   = NumericType.SByte;
            numTypeMap[typeof(short)]   = NumericType.Int16;
            numTypeMap[typeof(int)]     = NumericType.Int32;
            numTypeMap[typeof(long)]    = NumericType.Int64;
            numTypeMap[typeof(ushort)]  = NumericType.UInt16;
            numTypeMap[typeof(uint)]    = NumericType.UInt32;
            numTypeMap[typeof(ulong)]   = NumericType.UInt64;
            numTypeMap[typeof(float)]   = NumericType.Float;
            numTypeMap[typeof(double)]  = NumericType.Double;
            numTypeMap[typeof(decimal)] = NumericType.Decimal;
            g_numTypeMap                = numTypeMap;
        }

        public static NumericType GetNumericType(this Type type)
        {
            if (g_numTypeMap.TryGetValue(type, out NumericType value))
            {
                return value;
            }

            return NumericType.NaN;
        }

        /// <summary>
        /// 判断一个类型是否是数值类型
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsNumericType(this Type type)
        {
            return g_numTypeMap.ContainsKey(type);

            //也可以这样
            //return !o.IsClass && !o.IsInterface && o.GetInterfaces().Any(q => q == typeof(IFormattable));
        }

        public static bool IsNullableNumericType(this Type type)
        {
            return type.Name.StartsWith("Nullable") && type.GetGenericArguments()[0].IsNumericType();
        }

        /// <summary>
        /// 得到字段值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type"></param>
        /// <param name="fieldName"></param>
        /// <param name="value"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool GetFieldValue<T>(this Type type, string fieldName, out T value, object obj = null)
        {
            var field = type.GetField(fieldName);
            if (field == null)
            {
                value = default;
                return false;
            }

            value = (T)field.GetValue(obj);
            return true;
        }

        public static bool SetFieldValue(this Type type,        string name, object value,
                                         object    inst = null, BindingFlags bindingArray =
                                                 BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public)
        {
            var field = type.GetField(name, bindingArray);
            if (field == null)
            {
                return false;
            }

            field.SetValue(inst, value);
            return true;
        }

        public static object GetPropertyValue(this Type type, string name, object inst = null,
                                              BindingFlags bindingArray =
                                                      BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public)
        {
            var instProp = type.GetProperty(name, bindingArray);
            if (instProp == null)
            {
                return null;
            }

            return instProp.GetValue(inst);
        }

        public static TReturn StaticCall<TReturn>(this Type type, string methodName, object[] param = null,
                                                  TReturn   defReturn = default)
        {
            var info = type.GetMethod(methodName, BindingFlags.Static |
                                      BindingFlags.Public |
                                      BindingFlags.NonPublic |
                                      BindingFlags.FlattenHierarchy);
            if (info == null)
            {
                return defReturn;
            }

            return (TReturn)info.Invoke(null, param);
        }
    }

    public enum NumericType
    {
        NaN,
        Byte,
        SByte,
        Int16,
        Int32,
        Int64,
        UInt16,
        UInt32,
        UInt64,
        Float,
        Double,
        Decimal
    }
}
