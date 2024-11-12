#if UNITY_EDITOR
#define NUMERIC_CHECK_SYMBOLS //数值检查宏 离开Unity要使用就在unity设置中添加这个宏
#endif
using System;

namespace ET
{
    /// <summary>
    /// 扩展方法
    /// </summary>
    public static class NumericChangeHelper
    {
        public static NumericChange Create(Entity changeEntity, int numericType, long oldValue, long newValue)
        {
            return new NumericChange { _ChangeEntity = changeEntity, _NumericType = numericType, _Old = oldValue, _New = newValue };
        }

        public static Entity GetEntity(this NumericChange self)
        {
            if (self._ChangeEntity is { IsDisposed: false })
            {
                return self._ChangeEntity;
            }

            Log.Error($"目标为空 {self._ChangeEntity == null} 或 已摧毁 {self._ChangeEntity.IsDisposed} 请检查");
            return null;
        }

        public static T GetEntity<T>(this NumericChange self) where T : Entity
        {
            var entity = self.GetEntity();
            switch (entity)
            {
                case null:
                    return null;
                case T targetEntity:
                    return targetEntity;
                default:
                    Log.Error($"目标类型不匹配 {entity.GetType().Name} != {typeof(T).Name} ");
                    return null;
            }
        }

        public static Type GetEntityType(this NumericChange self)
        {
            var entity = self.GetEntity();
            if (entity != null)
            {
                return entity.GetType();
            }

            return null;
        }

        public static long GetEntityId(this NumericChange self)
        {
            var entity = self.GetEntity();
            return entity?.Id ?? 0;
        }

        public static ENumericType GetNumericTypeEnum(this NumericChange self)
        {
            return (ENumericType)self._NumericType;
        }

        public static int GetNumericType(this NumericChange self)
        {
            return self._NumericType;
        }

        #region 获取当前值

        public static bool GetAsBool(this NumericChange self)
        {
            #if NUMERIC_CHECK_SYMBOLS
            self._NumericType.CheckGetNumeric(ENumericValueType.Bool);
            #endif
            return self._New != 0;
        }

        public static float GetAsFloat(this NumericChange self)
        {
            #if NUMERIC_CHECK_SYMBOLS
            self._NumericType.CheckGetNumeric(ENumericValueType.Float);
            #endif
            return self._New / NumericConst.FloatRate;
        }

        public static int GetAsInt(this NumericChange self)
        {
            #if NUMERIC_CHECK_SYMBOLS
            self._NumericType.CheckGetNumeric(ENumericValueType.Int);
            #endif
            return (int)self._New;
        }

        public static long GetAsLong(this NumericChange self)
        {
            #if NUMERIC_CHECK_SYMBOLS
            self._NumericType.CheckGetNumeric(ENumericValueType.Long);
            #endif
            return self._New;
        }

        public static long GetSourceValue(this NumericChange self)
        {
            return self._New;
        }

        #endregion

        #region 获取改变之前的值

        public static bool GetAsBoolOld(this NumericChange self)
        {
            #if NUMERIC_CHECK_SYMBOLS
            self._NumericType.CheckGetNumeric(ENumericValueType.Bool);
            #endif
            return self._Old != 0;
        }

        public static float GetAsFloatOld(this NumericChange self)
        {
            #if NUMERIC_CHECK_SYMBOLS
            self._NumericType.CheckGetNumeric(ENumericValueType.Float);
            #endif
            return self._Old / NumericConst.FloatRate;
        }

        public static int GetAsIntOld(this NumericChange self)
        {
            #if NUMERIC_CHECK_SYMBOLS
            self._NumericType.CheckGetNumeric(ENumericValueType.Int);
            #endif
            return (int)self._Old;
        }

        public static long GetAsLongOld(this NumericChange self)
        {
            #if NUMERIC_CHECK_SYMBOLS
            self._NumericType.CheckGetNumeric(ENumericValueType.Long);
            #endif
            return self._Old;
        }

        public static long GetSourceValueOld(this NumericChange self)
        {
            return self._Old;
        }

        #endregion

        #region 获取改变值

        public static bool GetChangeAsBool(this NumericChange self)
        {
            #if NUMERIC_CHECK_SYMBOLS
            self._NumericType.CheckGetNumeric(ENumericValueType.Bool);
            #endif
            return self._New != 0;
        }

        public static float GetChangeAsFloat(this NumericChange self)
        {
            #if NUMERIC_CHECK_SYMBOLS
            self._NumericType.CheckGetNumeric(ENumericValueType.Float);
            #endif
            return (self._New - self._Old) / NumericConst.FloatRate;
        }

        public static int GetChangeAsInt(this NumericChange self)
        {
            #if NUMERIC_CHECK_SYMBOLS
            self._NumericType.CheckGetNumeric(ENumericValueType.Int);
            #endif
            return (int)(self._New - self._Old);
        }

        public static long GetChangeAsLong(this NumericChange self)
        {
            #if NUMERIC_CHECK_SYMBOLS
            self._NumericType.CheckGetNumeric(ENumericValueType.Long);
            #endif
            return self._New - self._Old;
        }

        #endregion
    }
}
