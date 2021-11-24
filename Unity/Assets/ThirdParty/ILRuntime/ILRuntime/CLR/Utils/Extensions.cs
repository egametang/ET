using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ILRuntime.CLR.TypeSystem;
using ILRuntime.CLR.Method;
using ILRuntime.Other;
using ILRuntime.Mono.Cecil;
using ILRuntime.Runtime.Intepreter;
using System.Reflection;

namespace ILRuntime.CLR.Utils
{
    public delegate TResult Func<T1, T2, T3, T4, T5, TResult>(T1 a1, T2 a2, T3 a3, T4 a4, T5 a5);

    public static class Extensions
    {
        public static List<IType> EmptyParamList = new List<IType>();
        public static List<IType> GetParamList(this MethodReference def, ILRuntime.Runtime.Enviorment.AppDomain appdomain, IType contextType, IMethod contextMethod, IType[] genericArguments)
        {
            if (def.HasParameters)
            {
                List<IType> param = new List<IType>();
                var dt = appdomain.GetType(def.DeclaringType, contextType, contextMethod);
                foreach (var i in def.Parameters)
                {
                    IType t = null;
                    t = appdomain.GetType(i.ParameterType, dt, null);
                    if ((t == null && def.IsGenericInstance) || (t != null && t.HasGenericParameter))
                    {
                        GenericInstanceMethod gim = (GenericInstanceMethod)def;
                        string name = i.ParameterType.IsByReference ? ((ByReferenceType)i.ParameterType).ElementType.FullName : i.ParameterType.FullName;
                        
                        for (int j = 0; j < gim.GenericArguments.Count; j++)
                        {
                            var gp = gim.ElementMethod.GenericParameters[j];
                            var ga = gim.GenericArguments[j];
                            if (name == gp.Name)
                            {
                                t = appdomain.GetType(ga, contextType, contextMethod);
                                if (t == null && genericArguments != null)
                                    t = genericArguments[j];
                                break;
                            }
                            else if (name.Contains(gp.Name))
                            {
                                t = appdomain.GetType(ga, contextType, contextMethod);
                                if (t == null && genericArguments != null)
                                    t = genericArguments[j];
                                if (name == gp.Name + "[]")
                                {
                                    name = t.FullName + "[]";
                                }
                                else
                                {
                                    /*name = name.Replace("<" + gp.Name + ">", "<" + ga.FullName + ">");
                                    name = name.Replace("<" + gp.Name + "[", "<" + ga.FullName + "[");
                                    name = name.Replace("<" + gp.Name + ",", "<" + ga.FullName + ",");
                                    name = name.Replace("," + gp.Name + ">", "," + ga.FullName + ">");
                                    name = name.Replace("," + gp.Name + "[", "," + ga.FullName + "[");
                                    name = name.Replace("," + gp.Name + ",", "," + ga.FullName + ",");
                                    name = name.Replace("," + gp.Name + "[", "," + ga.FullName + "[");*/
                                    name = ReplaceGenericArgument(name, gp.Name, t.FullName);
                                }
                                t = null;
                            }
                        }
                        if(dt.GenericArguments != null)
                        {
                            foreach(var gp in dt.GenericArguments)
                            {
                                if (name.Contains(gp.Key))
                                {
                                    name = ReplaceGenericArgument(name, gp.Key, gp.Value.FullName);
                                }
                            }
                        }
                        if (t == null)
                            t = appdomain.GetType(name);
                        if (t != null && i.ParameterType.IsByReference)
                            t = t.MakeByRefType();
                    }

                    param.Add(t);
                }
                return param;
            }
            else
                return EmptyParamList;
        }

        static string ReplaceGenericArgument(string typename, string argumentName, string argumentType, bool isGA = false)
        {
            string baseType;
            StringBuilder sb = new StringBuilder();
            List<string> ga;
            bool isArray;
            byte rank;
            Runtime.Enviorment.AppDomain.ParseGenericType(typename, out baseType, out ga, out isArray, out rank);
            string baseTypeQualification = null;
            bool hasGA = ga != null && ga.Count > 0;
            if (baseType == argumentName)
            {
                bool isAssemblyQualified = argumentName.Contains('=') || argumentType.Contains('=');
                if (isGA && isAssemblyQualified)
                    sb.Append('[');
                sb.Append(argumentType);
                if (isGA && isAssemblyQualified)
                    sb.Append(']');
            }
            else
            {
                bool isAssemblyQualified = baseType.Contains('=');
                if (isGA && !hasGA && isAssemblyQualified)
                    sb.Append('[');
                else if (isAssemblyQualified)
                {
                    sb.Append('[');
                    baseTypeQualification = baseType.Substring(baseType.IndexOf(','));
                    baseType = baseType.Substring(0, baseType.IndexOf(','));                    
                }
                sb.Append(baseType);
                if (isGA && !hasGA && isAssemblyQualified)
                    sb.Append(']');
            }
            if (hasGA)
            {
                sb.Append("[");
                bool isFirst = true;
                foreach (var i in ga)
                {
                    if (isFirst)
                        isFirst = false;
                    else
                        sb.Append(",");
                    sb.Append(ReplaceGenericArgument(i, argumentName, argumentType, true));
                }
                sb.Append("]");
            }
            if (!string.IsNullOrEmpty(baseTypeQualification))
            {
                sb.Append(baseTypeQualification);
                sb.Append(']');
            }
            if (isArray)
            {
                sb.Append("[");
                for (int i = 0; i < rank - 1; i++)
                    sb.Append(",");
                sb.Append("]");
            }
            return sb.ToString();
        }

        [Flags]
        public enum TypeFlags
        {
            Default = 0,
            IsPrimitive = 0x1,
            IsByRef = 0x2,
            IsEnum = 0x4,
            IsDelegate = 0x8,
            IsValueType = 0x10,
        }

        private static readonly Dictionary<Type, TypeFlags> typeFlags = new Dictionary<Type, TypeFlags>(new ByReferenceKeyComparer<Type>());

        public static bool FastIsEnum(this Type pt)
        {
            return (pt.GetTypeFlags() & TypeFlags.IsEnum) != 0;
        }

        public static bool FastIsByRef(this Type pt)
        {
            return (pt.GetTypeFlags() & TypeFlags.IsByRef) != 0;
        }

        public static bool FastIsPrimitive(this Type pt)
        {
            return (pt.GetTypeFlags() & TypeFlags.IsPrimitive) != 0;
        }

        public static bool FastIsValueType(this Type pt)
        {
            return (pt.GetTypeFlags() & TypeFlags.IsValueType) != 0;
        }

        public static TypeFlags GetTypeFlagsRecursive(this Type pt)
        {
            var res = GetTypeFlags(pt);
            if ((res & TypeFlags.IsByRef) == TypeFlags.IsByRef)
                res = GetTypeFlagsRecursive(pt.GetElementType());
            return res;
        }

        public static TypeFlags GetTypeFlags(this Type pt)
        {
            var result = TypeFlags.Default;

            if (!typeFlags.TryGetValue(pt, out result))
            {
                if (pt.IsPrimitive)
                {
                    result |= TypeFlags.IsPrimitive;
                }

                if (pt == typeof(Delegate) || pt.IsSubclassOf(typeof(Delegate)))
                {
                    result |= TypeFlags.IsDelegate;
                }

                if (pt.IsByRef)
                {
                    result |= TypeFlags.IsByRef;
                }

                if (pt.IsEnum)
                {
                    result |= TypeFlags.IsEnum;
                }

                if (pt.IsValueType)
                {
                    result |= TypeFlags.IsValueType;
                }

                typeFlags[pt] = result;
            }

            return result;
        }

        public static object CheckCLRTypes(this Type pt, object obj)
        {
            var typeFlags = GetTypeFlags(pt);
            return CheckCLRTypes(pt, obj, typeFlags);
        }

        public static object CheckCLRTypes(this Type pt, object obj, TypeFlags typeFlags)
        {
            if (obj == null)
                return null;

            if ((typeFlags & TypeFlags.IsPrimitive) != 0)
            {
                if (pt == typeof(int)) return obj;
                if (pt == typeof(bool) && !(obj is bool))
                {
                    obj = (int)obj == 1;
                }
                else if (pt == typeof(byte) && !(obj is byte))
                    obj = (byte)(int)obj;
                else if (pt == typeof(short) && !(obj is short))
                    obj = (short)(int)obj;
                else if (pt == typeof(char) && !(obj is char))
                    obj = (char)(int)obj;
                else if (pt == typeof(ushort) && !(obj is ushort))
                    obj = (ushort)(int)obj;
                else if (pt == typeof(uint) && !(obj is uint))
                    obj = (uint)(int)obj;
                else if (pt == typeof(sbyte) && !(obj is sbyte))
                    obj = (sbyte)(int)obj;
                else if (pt == typeof(ulong) && !(obj is ulong))
                {
                    obj = (ulong)(long)obj;
                }
            }
            else if (obj is ILRuntime.Reflection.ILRuntimeWrapperType)
            {
                obj = ((ILRuntime.Reflection.ILRuntimeWrapperType)obj).RealType;
            }
            else if ((typeFlags & TypeFlags.IsDelegate) != 0)
            {
                if (obj is Delegate)
                    return obj;
                if (pt == typeof(Delegate))
                    return ((IDelegateAdapter)obj).Delegate;
                return ((IDelegateAdapter)obj).GetConvertor(pt);
            }
            else if ((typeFlags & TypeFlags.IsByRef) != 0)
            {
                return CheckCLRTypes(pt.GetElementType(), obj);
            }
            else if ((typeFlags & TypeFlags.IsEnum) != 0)
            {
                return Enum.ToObject(pt, obj);
            }
            else if (obj is ILTypeInstance)
            {
                var adapter = obj as IDelegateAdapter;

                if (adapter != null && pt != typeof(ILTypeInstance))
                {
                    return adapter.Delegate;
                }

                if (!(obj is ILEnumTypeInstance))
                {
                    var ins = (ILTypeInstance)obj;
                    /*if (ins.IsValueType)
                        ins = ins.Clone();*/
                    return ins.CLRInstance;
                }
            }
            return obj;
        }

        public static bool CheckMethodParams(this MethodInfo m, Type[] args)
        {
            var arr = m.GetParameters();
            if (arr.Length != args.Length) return false;
            for (var i = 0; i < args.Length; i++)
            {
                var t1 = arr[i].ParameterType;
                var t2 = args[i];
                if (t1 != t2 || t1.IsByRef != t2.IsByRef)
                    return false;
            }
            return true;
        }

        public static bool CheckMethodParams(this MethodInfo m, ParameterInfo[] args)
        {
            var arr = m.GetParameters();
            if (arr.Length != args.Length) return false;
            for (var i = 0; i < args.Length; i++)
            {
                var t1 = arr[i].ParameterType;
                var t2 = args[i].ParameterType;
                if (t1 != t2 || t1.IsByRef != t2.IsByRef)
                    return false;
            }
            return true;
        }
    }
}
