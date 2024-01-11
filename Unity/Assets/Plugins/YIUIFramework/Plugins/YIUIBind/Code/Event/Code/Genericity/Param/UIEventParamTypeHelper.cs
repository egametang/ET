using System;
using System.Collections.Generic;
using UnityEngine;
using Logger = YIUIFramework.Logger;

namespace YIUIFramework
{
    public static class UIEventParamTypeHelper
    {
        public static Type GetParamType(this EUIEventParamType paramType)
        {
            switch (paramType)
            {
                case EUIEventParamType.ParamVo:
                    return typeof(UIBindParamVo);
                case EUIEventParamType.Object:
                    return typeof(object);
                case EUIEventParamType.Bool:
                    return typeof(bool);
                case EUIEventParamType.String:
                    return typeof(string);
                case EUIEventParamType.Int:
                    return typeof(int);
                case EUIEventParamType.Float:
                    return typeof(float);
                case EUIEventParamType.Vector3:
                    return typeof(Vector3);
                case EUIEventParamType.List_Int:
                    return typeof(List<int>);
                case EUIEventParamType.List_Long:
                    return typeof(List<long>);
                case EUIEventParamType.List_String:
                    return typeof(List<string>);
                case EUIEventParamType.Long:
                    return typeof(long);
                case EUIEventParamType.Uint:
                    return typeof(uint);
                case EUIEventParamType.Ulong:
                    return typeof(ulong);
                case EUIEventParamType.Double:
                    return typeof(double);
                case EUIEventParamType.Vector2:
                    return typeof(Vector2);
                case EUIEventParamType.UnityGameObject:
                    return typeof(GameObject);
                default:
                    Logger.LogError($"没有实现这个新类型 {paramType}");
                    return typeof(int);
            }
        }

        public static string GetParamTypeString(this EUIEventParamType paramType)
        {
            switch (paramType)
            {
                case EUIEventParamType.ParamVo:
                    return "UIBindParamVo";
                case EUIEventParamType.Object:
                    return "object";
                case EUIEventParamType.Bool:
                    return "bool";
                case EUIEventParamType.String:
                    return "string";
                case EUIEventParamType.Int:
                    return "int";
                case EUIEventParamType.Float:
                    return "float";
                case EUIEventParamType.Vector3:
                    return "Vector3";
                case EUIEventParamType.List_Int:
                    return "List<int>";
                case EUIEventParamType.List_Long:
                    return "List<long>";
                case EUIEventParamType.List_String:
                    return "List<string>";
                case EUIEventParamType.Long:
                    return "long";
                case EUIEventParamType.Uint:
                    return "uint";
                case EUIEventParamType.Ulong:
                    return "ulong";
                case EUIEventParamType.Double:
                    return "double";
                case EUIEventParamType.Vector2:
                    return "Vector2";
                case EUIEventParamType.UnityGameObject:
                    return "GameObject";
                default:
                    Logger.LogError($"没有实现这个新类型 {paramType}");
                    return "";
            }
        }

        public static string GetParamTypeFullString(this EUIEventParamType paramType)
        {
            switch (paramType)
            {
                case EUIEventParamType.ParamVo:
                    return "YIUIBind.UIBindParamVo";
                case EUIEventParamType.Object:
                    return "System.object";
                case EUIEventParamType.Bool:
                    return "System.bool";
                case EUIEventParamType.String:
                    return "System.string";
                case EUIEventParamType.Int:
                    return "System.int";
                case EUIEventParamType.Float:
                    return "System.float";
                case EUIEventParamType.Vector3:
                    return "UnityEngine.Vector3";
                case EUIEventParamType.List_Int:
                    return "System.Collections.Generic.List<int>";
                case EUIEventParamType.List_Long:
                    return "System.Collections.Generic.List<long>";
                case EUIEventParamType.List_String:
                    return "System.Collections.Generic.List<string>";
                case EUIEventParamType.Long:
                    return "System.long";
                case EUIEventParamType.Uint:
                    return "System.uint";
                case EUIEventParamType.Ulong:
                    return "System.ulong";
                case EUIEventParamType.Double:
                    return "System.double";
                case EUIEventParamType.Vector2:
                    return "UnityEngine.Vector2";
                case EUIEventParamType.UnityGameObject:
                    return "UnityEngine.GameObject";
                default:
                    Logger.LogError($"没有实现这个新类型 {paramType}");
                    return "";
            }
        }
    }
}