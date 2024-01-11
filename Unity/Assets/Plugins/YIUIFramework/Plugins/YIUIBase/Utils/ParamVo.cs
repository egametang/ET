using System;
using System.Collections;

namespace YIUIFramework
{
    public class ParamVo
    {
        private static readonly SimplePool<ParamVo> g_cacheParam = new SimplePool<ParamVo>(5);

        /// <summary>
        /// 原始数据
        /// </summary>
        public object Data;

        public ParamVo()
        {
        }

        public ParamVo(object data)
        {
            Data = data;
        }

        public T Get<T>(int index = 0, T defaultValue = default(T))
        {
            T result;
            Get(out result, index, defaultValue);
            return result;
        }

        /// <summary>
        /// 取出参数列表的第index个
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="outResult"></param>
        /// <param name="index"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public ParamGetResult Get<T>(out T outResult, int index = 0, T defaultValue = default(T))
        {
            if (Data == null)
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
            if (Data is IList)
            {
                IList datas = (IList)Data;
                if (index >= datas.Count)
                {
                    outResult = defaultValue;
                    return ParamGetResult.IndexIsOut;
                }

                data = datas[index];
            }
            else
            {
                if (index != 0)
                {
                    outResult = defaultValue;
                    return ParamGetResult.IndexIsOut;
                }

                data = Data;
            }

            //如果数据是字符串，并且相要的值是数值类型，则自动转换
            if (data is string && typeof(T).IsNumericType())
            {
                if (StrConv.ToNumber<T>((string)data, out outResult))
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

        public void Reset()
        {
            Data = null;
        }

        public static ParamVo Get(object data = null)
        {
            ParamVo paramVo = g_cacheParam.Get();
            paramVo.Data = data;
            return paramVo;
        }

        public static void Put(ParamVo value)
        {
            if (value != null)
            {
                value.Data = null;
                g_cacheParam.Put(value);
            }
        }
    }
    
}