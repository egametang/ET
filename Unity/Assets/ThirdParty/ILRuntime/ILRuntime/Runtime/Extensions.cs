using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ILRuntime.Runtime.Stack;

namespace ILRuntime.Runtime
{
    static class Extensions
    {
        public static void GetClassName(this Type type, out string clsName, out string realClsName, out bool isByRef, bool simpleClassName = false)
        {
            isByRef = type.IsByRef;
            int arrayRank = 1;
            bool isArray = type.IsArray;
            if (isByRef)
            {
                type = type.GetElementType();
            }
            if (isArray)
            {
                arrayRank = type.GetArrayRank();
                type = type.GetElementType();
                if (type.IsArray)
                {
                    type.GetClassName(out clsName, out realClsName, out isByRef, simpleClassName);

                    clsName += "_Array";
                    if (!simpleClassName)
                        clsName += "_Binding";
                    if (arrayRank > 1)
                        clsName += arrayRank;
                    if (arrayRank <= 1)
                        realClsName += "[]";
                    else
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.Append(realClsName);
                        sb.Append('[');
                        for (int i = 0; i < arrayRank - 1; i++)
                        {
                            sb.Append(',');
                        }
                        sb.Append(']');
                        realClsName = sb.ToString();
                    }

                    return;
                }
            }
            string realNamespace = null;
            bool isNestedGeneric = false;
            if (type.IsNested)
            {
                string bClsName, bRealClsName;
                bool tmp;
                var rt = type.ReflectedType;
                if(rt.IsGenericType && rt.IsGenericTypeDefinition)
                {
                    if (type.IsGenericType)
                    {
                        rt = rt.MakeGenericType(type.GetGenericArguments());
                        isNestedGeneric = true;
                    }
                }
                GetClassName(rt, out bClsName, out bRealClsName, out tmp);
                clsName = bClsName + "_";
                realNamespace = bRealClsName + ".";
            }
            else
            {
                clsName = simpleClassName ? "" : (!string.IsNullOrEmpty(type.Namespace) ? type.Namespace.Replace(".", "_") + "_" : "");
                realNamespace = !string.IsNullOrEmpty(type.Namespace) ? type.Namespace + "." : null;
            }
            clsName = clsName + type.Name.Replace(".", "_").Replace("`", "_").Replace("<", "_").Replace(">", "_");
            bool isGeneric = false;
            string ga = null;
            if (type.IsGenericType && !isNestedGeneric)
            {
                isGeneric = true;
                clsName += "_";
                ga = "<";
                var args = type.GetGenericArguments();
                bool first = true;
                foreach (var j in args)
                {
                    if (first)
                        first = false;
                    else
                    {
                        clsName += "_";
                        ga += ", ";
                    }
                    string a, b;
                    bool tmp;
                    GetClassName(j, out a, out b, out tmp, true);
                    clsName += a;
                    ga += b;
                }
                ga += ">";
            }
            if (isArray)
            {
                clsName += "_Array";
                if (arrayRank > 1)
                    clsName += arrayRank;
            }
            if (!simpleClassName)
                clsName += "_Binding";

            realClsName = realNamespace;
            if (isGeneric)
            {
                int idx = type.Name.IndexOf("`");
                if (idx > 0)
                {
                    realClsName += type.Name.Substring(0, idx);
                    realClsName += ga;
                }
                else
                    realClsName += type.Name;
            }
            else
                realClsName += type.Name;

            if (isArray)
            {
                if (arrayRank <= 1)
                    realClsName += "[]";
                else
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append(realClsName);
                    sb.Append('[');
                    for(int i=0;i<arrayRank - 1; i++)
                    {
                        sb.Append(',');
                    }
                    sb.Append(']');
                    realClsName = sb.ToString();
                }
            }

        }
        public static int ToInt32(this object obj)
        {
            if (obj is int)
                return (int)obj;
            if (obj is float)
                return (int)(float)obj;
            if (obj is long)
                return (int)(long)obj;
            if (obj is short)
                return (int)(short)obj;
            if (obj is double)
                return (int)(double)obj;
            if (obj is byte)
                return (int)(byte)obj;
            if (obj is Intepreter.ILEnumTypeInstance)
                return (int)((Intepreter.ILEnumTypeInstance)obj)[0];
            if (obj is uint)
                return (int)(uint)obj;
            if (obj is ushort)
                return (int)(ushort)obj;
            if (obj is sbyte)
                return (int)(sbyte)obj;
            throw new InvalidCastException();
        }
        public static long ToInt64(this object obj)
        {
            if (obj is long)
                return (long)obj;
            if (obj is int)
                return (long)(int)obj;
            if (obj is float)
                return (long)(float)obj;
            if (obj is short)
                return (long)(short)obj;
            if (obj is double)
                return (long)(double)obj;
            if (obj is byte)
                return (long)(byte)obj;
            if (obj is uint)
                return (long)(uint)obj;
            if (obj is ushort)
                return (long)(ushort)obj;
            if (obj is sbyte)
                return (long)(sbyte)obj;
            throw new InvalidCastException();
        }
        public static short ToInt16(this object obj)
        {
            if (obj is short)
                return (short)obj;
            if (obj is long)
                return (short)(long)obj;
            if (obj is int)
                return (short)(int)obj;
            if (obj is float)
                return (short)(float)obj;
            if (obj is double)
                return (short)(double)obj;
            if (obj is byte)
                return (short)(byte)obj;
            if (obj is uint)
                return (short)(uint)obj;
            if (obj is ushort)
                return (short)(ushort)obj;
            if (obj is sbyte)
                return (short)(sbyte)obj;
            throw new InvalidCastException();
        }
        public static float ToFloat(this object obj)
        {
            if (obj is float)
                return (float)obj;
            if (obj is int)
                return (float)(int)obj;
            if (obj is long)
                return (float)(long)obj;
            if (obj is short)
                return (float)(short)obj;
            if (obj is double)
                return (float)(double)obj;
            if (obj is byte)
                return (float)(byte)obj;
            if (obj is uint)
                return (float)(uint)obj;
            if (obj is ushort)
                return (float)(ushort)obj;
            if (obj is sbyte)
                return (float)(sbyte)obj;
            throw new InvalidCastException();
        }

        public static double ToDouble(this object obj)
        {
            if (obj is double)
                return (double)obj;
            if (obj is float)
                return (float)obj;
            if (obj is int)
                return (double)(int)obj;
            if (obj is long)
                return (double)(long)obj;
            if (obj is short)
                return (double)(short)obj;
            if (obj is byte)
                return (double)(byte)obj;
            if (obj is uint)
                return (double)(uint)obj;
            if (obj is ushort)
                return (double)(ushort)obj;
            if (obj is sbyte)
                return (double)(sbyte)obj;
            throw new InvalidCastException();
        }

        public static Type GetActualType(this object value)
        {
            if (value is ILRuntime.Runtime.Enviorment.CrossBindingAdaptorType)
                return ((ILRuntime.Runtime.Enviorment.CrossBindingAdaptorType)value).ILInstance.Type.ReflectionType;
            if (value is ILRuntime.Runtime.Intepreter.ILTypeInstance)
                return ((ILRuntime.Runtime.Intepreter.ILTypeInstance)value).Type.ReflectionType;
            else
                return value.GetType();
        }
    }
}
