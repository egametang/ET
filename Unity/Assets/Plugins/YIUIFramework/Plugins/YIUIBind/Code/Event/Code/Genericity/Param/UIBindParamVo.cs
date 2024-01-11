using System;
using System.Collections;
using YIUIFramework;

namespace YIUIFramework
{
    public class UIBindParamVo
    {
        private static readonly ObjectPool<UIBindParamVo> g_CacheParam =
            new ObjectPool<UIBindParamVo>(null, l => l.Reset());

        /// <summary>
        /// 原始数据
        /// </summary>
        private object m_Data;

        public UIBindParamVo()
        {
        }

        public UIBindParamVo(object data)
        {
            m_Data = data;
        }

        public T Get<T>(int index = 0, T defaultValue = default(T))
        {
            Get(out var result, index, defaultValue);
            return result;
        }

        private ParamGetResult Get<T>(out T outResult, int index = 0, T defaultValue = default(T))
        {
            if (m_Data == null)
            {
                outResult = defaultValue;
                return ParamGetResult.DataIsNull;
            }

            if (index < 0)
            {
                outResult = defaultValue;
                return ParamGetResult.IndexIsLess;
            }

            object data;
            if (m_Data is IList dataList)
            {
                if (index >= dataList.Count)
                {
                    outResult = defaultValue;
                    return ParamGetResult.IndexIsOut;
                }

                data = dataList[index];
            }
            else
            {
                if (index != 0)
                {
                    outResult = defaultValue;
                    return ParamGetResult.IndexIsOut;
                }

                data = m_Data;
            }

            //如果数据是字符串，并且相要的值是数值类型，则自动转换
            if (data is string s && typeof(T).IsNumericType())
            {
                if (StrConv.ToNumber<T>(s, out outResult))
                {
                    return ParamGetResult.Success;
                }

                outResult = defaultValue;
                return ParamGetResult.ParseErr;
            }

            try
            {
                outResult = (T)data;
                return ParamGetResult.Success;
            }
            catch (Exception)
            {
                outResult = defaultValue;
                return ParamGetResult.ParseErr;
            }
        }

        private void Reset()
        {
            m_Data = null;
        }

        public static UIBindParamVo Get(object data = null)
        {
            var paramVo = g_CacheParam.Get();
            paramVo.m_Data = data;
            return paramVo;
        }

        public static void Put(UIBindParamVo value)
        {
            if (value == null) return;
            value.m_Data = null;
            g_CacheParam.Release(value);
        }
    }

    public enum ParamGetResult
    {
        Success,
        DataIsNull,
        IndexIsLess,
        IndexIsOut,
        ParseErr
    }
}