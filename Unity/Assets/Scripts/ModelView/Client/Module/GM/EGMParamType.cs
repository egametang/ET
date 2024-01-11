using Sirenix.OdinInspector;
using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// GM参数类型
    /// 目前支持的类型
    /// </summary>
    public enum EGMParamType
    {
        [LabelText("字符串")]
        String,

        [LabelText("布尔")]
        Bool,

        [LabelText("小数")]
        Float,

        [LabelText("整数")]
        Int,

        [LabelText("整数64")]
        Long,
    }
    
    public static class GMParamExtend
    {
        public static object TryToValue(this EGMParamType self, string value)
        {
            switch (self)
            {
                case EGMParamType.String:
                    return value;
                case EGMParamType.Bool:
                    if (string.IsNullOrEmpty(value) || value == "0")
                        return false;
                    return true;
                case EGMParamType.Float:
                    if (string.IsNullOrEmpty(value))
                        return 0f;
                    if (!float.TryParse(value,out var floatValue))
                        Debug.LogError($"参数转换失败 {value} 无法转换 float 请检查");
                    return floatValue;
                case EGMParamType.Int:
                    if (string.IsNullOrEmpty(value))
                        return 0;
                    if (!int.TryParse(value,out var intValue))
                        Debug.LogError($"参数转换失败 {value} 无法转换 int 请检查");
                    return intValue;
                case EGMParamType.Long:
                    if (string.IsNullOrEmpty(value))
                        return 0;
                    if (!long.TryParse(value,out var longValue))
                        Debug.LogError($"参数转换失败 {value} 无法转换 long 请检查");
                    return longValue;
                default:
                    Debug.LogError($"未知类型 无法转换");
                    break;
            }
            return value;
        }
        
    }
    
}