using System;
using System.Collections.Generic;
using UnityEngine;
using Logger = YIUIFramework.Logger;

namespace YIUIFramework
{
    public static class UIDataHelper
    {
        #region New

        /// <summary>
        /// 根据类型获取一个新的数据
        /// </summary>
        public static UIDataValue GetNewDataValue(EUIBindDataType dataType)
        {
            //这个类型不会无限扩展所以暂时用这个方法
            //如果有更好的方法可以修改
            switch (dataType)
            {
                case EUIBindDataType.Bool:
                    return new UIDataValueBool();
                case EUIBindDataType.String:
                    return new UIDataValueString();
                case EUIBindDataType.Int:
                    return new UIDataValueInt();
                case EUIBindDataType.Float:
                    return new UIDataValueFloat();
                case EUIBindDataType.Vector3:
                    return new UIDataValueVector3();
                case EUIBindDataType.List_Int:
                    return new UIDataValueListInt();
                case EUIBindDataType.List_Long:
                    return new UIDataValueListLong();
                case EUIBindDataType.Long:
                    return new UIDataValueLong();
                case EUIBindDataType.Uint:
                    return new UIDataValueUInt();
                case EUIBindDataType.Vector2:
                    return new UIDataValueVector2();
                case EUIBindDataType.Color:
                    return new UIDataValueColor();
                case EUIBindDataType.Double:
                //其他类型还没需求就没写@L李
                case EUIBindDataType.List_String:
                case EUIBindDataType.Ulong:
                default:
                    Logger.LogError($"没有实现这个类型 {dataType}");
                    return new UIDataValueInt();
            }
        }

        public static UIDataValue GetNewDataValue(UIDataValue dataValue)
        {
            return GetNewDataValue(dataValue.UIBindDataType);
        }

        #endregion

        #region Get/Set Value

        private static UIDataValueBase<T> GetDataValueBase<T>(this UIDataValue self)
        {
            if (self is UIDataValueBase<T> finalResult) return finalResult;
            Logger.LogError($"获取值转型失败，当前类型是: {self.UIDataValueType} 不是：{typeof(T)}");
            return null;
        }

        public static T GetValue<T>(this UIData self)
        {
            return self.DataValue.GetValue<T>();
        }

        public static T GetValue<T>(this UIDataValue self)
        {
            var finalResult = GetDataValueBase<T>(self);
            return finalResult != null ? finalResult.GetValue() : default;
        }

        public static T GetValue<T>(this UIData self, T defaultValue)
        {
            return self.DataValue.GetValue<T>(defaultValue);
        }

        public static T GetValue<T>(this UIDataValue self, T defaultValue)
        {
            var finalResult = GetDataValueBase<T>(self);
            return finalResult != null ? finalResult.GetValue() : defaultValue;
        }

        public static string GetValueToString(this UIData self)
        {
            return GetValueToString(self.DataValue);
        }

        public static string GetValueToString(this UIDataValue self)
        {
            //由每个泛型子类实现ToString方法;
            return self.ToString();

            //以下为老的值tosytring 可能会存在某些引用类型无效的情况
            //保留不删除
            /*
            switch (self.UIBindDataType)
            {
                case EUIBindDataType.Bool:
                    return self.GetValue<bool>().ToString();
                case EUIBindDataType.String:
                    return self.GetValue<string>().ToString();
                case EUIBindDataType.Int:
                    return self.GetValue<int>().ToString();
                case EUIBindDataType.Float:
                    return self.GetValue<float>().ToString();
                case EUIBindDataType.Vector3:
                    return self.GetValue<Vector3>().ToString();
                case EUIBindDataType.List_Int:
                    return self.GetValue<List<int>>().ToString();
                case EUIBindDataType.List_Long:
                    return self.GetValue<List<long>>().ToString();
                case EUIBindDataType.List_String:
                    return self.GetValue<List<string>>().ToString();
                case EUIBindDataType.Long:
                    return self.GetValue<long>().ToString();
                case EUIBindDataType.Uint:
                    return self.GetValue<uint>().ToString();
                case EUIBindDataType.Ulong:
                    return self.GetValue<ulong>().ToString();
                case EUIBindDataType.Double:
                    return self.GetValue<double>().ToString();
                case EUIBindDataType.Vector2:
                    return self.GetValue<Vector2>().ToString();
                case EUIBindDataType.Color:
                    return self.GetValue<Color>().ToString();
                default:
                    Logger.LogError($"此类型未实现 {self.UIBindDataType}");
                    return "null";
            }
            */
        }

        public static object GetValueObject(this UIData self)
        {
            return GetValueObject(self.DataValue);
        }

        public static object GetValueObject(this UIDataValue self)
        {
            switch (self.UIBindDataType)
            {
                case EUIBindDataType.Bool:
                    return self.GetValue<bool>();
                case EUIBindDataType.String:
                    return self.GetValue<string>();
                case EUIBindDataType.Int:
                    return self.GetValue<int>();
                case EUIBindDataType.Float:
                    return self.GetValue<float>();
                case EUIBindDataType.Vector3:
                    return self.GetValue<Vector3>();
                case EUIBindDataType.List_Int:
                    return self.GetValue<List<int>>();
                case EUIBindDataType.List_Long:
                    return self.GetValue<List<long>>();
                case EUIBindDataType.List_String:
                    return self.GetValue<List<string>>();
                case EUIBindDataType.Long:
                    return self.GetValue<long>();
                case EUIBindDataType.Uint:
                    return self.GetValue<uint>();
                case EUIBindDataType.Ulong:
                    return self.GetValue<ulong>();
                case EUIBindDataType.Double:
                    return self.GetValue<double>();
                case EUIBindDataType.Vector2:
                    return self.GetValue<Vector2>();
                case EUIBindDataType.Color:
                    return self.GetValue<Color>();
                default:
                    Logger.LogError($"此类型未实现 {self.UIBindDataType}");
                    return "null";
            }
        }

        public static bool Set<T>(this UIData self, T value, bool force = false)
        {
            return self.DataValue.Set(value, force);
        }

        public static bool Set<T>(this UIDataValue self, T value, bool force = false)
        {
            var finalResult = GetDataValueBase<T>(self);
            if (finalResult == null) return false;

            finalResult.SetValue(value, force);
            return true;
        }

        public static bool SetValueFrom(this UIData self, UIDataValue dataValue)
        {
            return self.DataValue.SetValueFrom(dataValue);
        }

        public static bool SetValueFrom<T>(this UIData self, UIDataValue dataValue, bool force = false)
        {
            return self.DataValue.SetValueFrom<T>(dataValue, force);
        }

        public static bool SetValueFrom<T>(this UIDataValue self, UIDataValue dataValue, bool force = false)
        {
            var finalResult = GetDataValueBase<T>(self);
            if (finalResult == null) return false;

            finalResult.SetValueFrom(dataValue, force);
            return true;
        }

        #endregion

        #region ChangeAction

        public static void AddValueChangeAction(this UIData self, Action action)
        {
            if (self.DataValue == null)
            {
                Logger.LogError($"{self.Name} 这个数据没有值");
                return;
            }

            self.DataValue.AddValueChangeAction(action);
        }

        public static void RemoveValueChangeAction(this UIData self, Action action)
        {
            if (self.DataValue == null)
            {
                Logger.LogError($"{self.Name} 这个数据没有值");
                return;
            }

            self.DataValue.RemoveValueChangeAction(action);
        }

        public static void AddValueChangeAction<T>(this UIData self, Action<T, T> action) where T : struct
        {
            if (self.DataValue == null)
            {
                Logger.LogError($"{self.Name} 这个数据没有值");
                return;
            }

            if (self.DataValue is UIDataValueBase<T> dataBase)
            {
                dataBase.AddValueChangeAction(action);
            }
            else
            {
                Logger.LogError($"{self.Name} 这个数据值类型不是 {typeof(T).Name}");
            }
        }

        public static void RemoveValueChangeAction<T>(this UIData self, Action<T, T> action) where T : struct
        {
            if (self.DataValue == null)
            {
                Logger.LogError($"{self.Name} 这个数据没有值");
                return;
            }

            if (self.DataValue is UIDataValueBase<T> dataBase)
            {
                dataBase.RemoveValueChangeAction(action);
            }
            else
            {
                Logger.LogError($"{self.Name} 这个数据值类型不是 {typeof(T).Name}");
            }
        }

        #endregion
    }
}