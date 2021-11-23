using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Text;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.CLR.Utils;

namespace ILRuntime.Runtime.CLRBinding
{
    static class BindingGeneratorExtensions
    {
        internal static bool ShouldSkipField(this Type type, FieldInfo i)
        {
            if (i.IsPrivate)
                return true;
            //EventHandler is currently not supported
            if (i.IsSpecialName)
            {
                return true;
            }
            if (i.GetCustomAttributes(typeof(ObsoleteAttribute), true).Length > 0)
                return true;
            return false;
        }

        internal static bool ShouldSkipMethod(this Type type, MethodBase i)
        {
            if (i.IsPrivate)
                return true;
            if (i.IsGenericMethodDefinition)
                return true;
            if (i.IsConstructor && type.IsAbstract)
                return true;
            if (i is MethodInfo && ((MethodInfo)i).ReturnType.IsByRef)
                return true;
            //EventHandler is currently not supported
            var param = i.GetParameters();
            if (i.IsSpecialName)
            {
                string[] t = i.Name.Split('_');
                //if (t[0] == "add" || t[0] == "remove")
                //    return true;
                if (t[0] == "get" || t[0] == "set")
                {
                    Type[] ts;
                    var cnt = t[0] == "set" ? param.Length - 1 : param.Length;

                    if (cnt > 0)
                    {
                        ts = new Type[cnt];
                        for (int j = 0; j < cnt; j++)
                        {
                            ts[j] = param[j].ParameterType;
                        }
                    }
                    else
                        ts = new Type[0];
                    var prop = type.GetProperty(t[1], ts);
                    if (prop == null)
                    {
                        return true;
                    }
                    if (prop.GetCustomAttributes(typeof(ObsoleteAttribute), true).Length > 0)
                        return true;
                }
            }
            if (i.GetCustomAttributes(typeof(ObsoleteAttribute), true).Length > 0)
                return true;
            foreach (var j in param)
            {
                if (j.ParameterType.IsPointer)
                    return true;
            }
            return false;
        }

        internal static void AppendParameters(this ParameterInfo[] param, StringBuilder sb, bool isMultiArr = false, int skipLast = 0)
        {
            bool first = true;
            for (int i = 0; i < param.Length - skipLast; i++)
            {
                if (first)
                    first = false;
                else
                    sb.Append(", ");
                var j = param[i];
                if (j.IsOut && j.ParameterType.IsByRef)
                    sb.Append("out ");
                else if (j.IsIn && j.ParameterType.IsByRef)
                    sb.Append("in ");
                else if (j.ParameterType.IsByRef)
                    sb.Append("ref ");
                if (isMultiArr)
                {
                    sb.Append("a");
                    sb.Append(i + 1);
                }
                else
                {
                    sb.Append("@");
                    sb.Append(j.Name);
                }
            }
        }

        internal static void AppendArgumentCode(this Type p, StringBuilder sb, int idx, string name, List<Type> valueTypeBinders, bool isMultiArr, bool hasByRef, bool needFree)
        {
            string clsName, realClsName;
            bool isByRef;
            p.GetClassName(out clsName, out realClsName, out isByRef);
            var pt = p.IsByRef ? p.GetElementType() : p;
            string shouldFreeParam = hasByRef ? "false" : "true";

            if (pt.IsValueType && !pt.IsPrimitive && valueTypeBinders != null && valueTypeBinders.Contains(pt))
            {
                if (isMultiArr)
                    sb.AppendLine(string.Format("            {0} a{1} = new {0}();", realClsName, idx));
                else
                    sb.AppendLine(string.Format("            {0} @{1} = new {0}();", realClsName, name));

                sb.AppendLine(string.Format("            if (ILRuntime.Runtime.Generated.CLRBindings.s_{0}_Binder != null) {{", clsName));

                if (isMultiArr)
                    sb.AppendLine(string.Format("                ILRuntime.Runtime.Generated.CLRBindings.s_{1}_Binder.ParseValue(ref a{0}, __intp, ptr_of_this_method, __mStack, {2});", idx, clsName, shouldFreeParam));
                else
                    sb.AppendLine(string.Format("                ILRuntime.Runtime.Generated.CLRBindings.s_{1}_Binder.ParseValue(ref @{0}, __intp, ptr_of_this_method, __mStack, {2});", name, clsName, shouldFreeParam));

                sb.AppendLine("            } else {");

                if (isByRef)
                    sb.AppendLine("                ptr_of_this_method = ILIntepreter.GetObjectAndResolveReference(ptr_of_this_method);");
                if (isMultiArr)
                    sb.AppendLine(string.Format("                a{0} = {1};", idx, p.GetRetrieveValueCode(realClsName)));
                else
                    sb.AppendLine(string.Format("                @{0} = {1};", name, p.GetRetrieveValueCode(realClsName)));
                if (!hasByRef && needFree)
                    sb.AppendLine("                __intp.Free(ptr_of_this_method);");

                sb.AppendLine("            }");
            }
            else
            {
                if (isByRef)
                {
                    if (p.GetElementType().IsPrimitive)
                    {
                        if (pt == typeof(int) || pt == typeof(uint) || pt == typeof(short) || pt == typeof(ushort) || pt == typeof(byte) || pt == typeof(sbyte) || pt == typeof(char))
                        {
                            if (pt == typeof(int))
                                sb.AppendLine(string.Format("            {0} @{1} = __intp.RetriveInt32(ptr_of_this_method, __mStack);", realClsName, name));
                            else
                                sb.AppendLine(string.Format("            {0} @{1} = ({0})__intp.RetriveInt32(ptr_of_this_method, __mStack);", realClsName, name));
                        }
                        else if (pt == typeof(long) || pt == typeof(ulong))
                        {
                            if (pt == typeof(long))
                                sb.AppendLine(string.Format("            {0} @{1} = __intp.RetriveInt64(ptr_of_this_method, __mStack);", realClsName, name));
                            else
                                sb.AppendLine(string.Format("            {0} @{1} = ({0})__intp.RetriveInt64(ptr_of_this_method, __mStack);", realClsName, name));
                        }
                        else if (pt == typeof(float))
                        {
                            sb.AppendLine(string.Format("            {0} @{1} = __intp.RetriveFloat(ptr_of_this_method, __mStack);", realClsName, name));
                        }
                        else if (pt == typeof(double))
                        {
                            sb.AppendLine(string.Format("            {0} @{1} = __intp.RetriveDouble(ptr_of_this_method, __mStack);", realClsName, name));
                        }
                        else if (pt == typeof(bool))
                        {
                            sb.AppendLine(string.Format("            {0} @{1} = __intp.RetriveInt32(ptr_of_this_method, __mStack) == 1;", realClsName, name));
                        }
                        else
                            throw new NotSupportedException();
                    }
                    else if (p.GetElementType().IsEnum)
                    {
                        sb.AppendLine(string.Format("            {0} @{1} = ({0})__intp.RetriveInt32(ptr_of_this_method, __mStack);", realClsName, name));
                    }
                    else
                    {
                        sb.AppendLine(string.Format("            {0} @{1} = ({0})typeof({0}).CheckCLRTypes(__intp.RetriveObject(ptr_of_this_method, __mStack), (CLR.Utils.Extensions.TypeFlags){2});", realClsName, name, (int)p.GetTypeFlagsRecursive()));
                    }

                }
                else
                {
                    if (isMultiArr)
                        sb.AppendLine(string.Format("            {0} a{1} = {2};", realClsName, idx, p.GetRetrieveValueCode(realClsName)));
                    else
                        sb.AppendLine(string.Format("            {0} @{1} = {2};", realClsName, name, p.GetRetrieveValueCode(realClsName)));
                    if (!hasByRef && !p.IsPrimitive && needFree)
                        sb.AppendLine("            __intp.Free(ptr_of_this_method);");

                }
            }
        }

        internal static string GetRetrieveValueCode(this Type type, string realClsName)
        {
            if (type.IsByRef)
                type = type.GetElementType();
            if (type.IsPrimitive)
            {
                if (type == typeof(int))
                {
                    return "ptr_of_this_method->Value";
                }
                else if (type == typeof(long))
                {
                    return "*(long*)&ptr_of_this_method->Value";
                }
                else if (type == typeof(short))
                {
                    return "(short)ptr_of_this_method->Value";
                }
                else if (type == typeof(bool))
                {
                    return "ptr_of_this_method->Value == 1";
                }
                else if (type == typeof(ushort))
                {
                    return "(ushort)ptr_of_this_method->Value";
                }
                else if (type == typeof(float))
                {
                    return "*(float*)&ptr_of_this_method->Value";
                }
                else if (type == typeof(double))
                {
                    return "*(double*)&ptr_of_this_method->Value";
                }
                else if (type == typeof(byte))
                {
                    return "(byte)ptr_of_this_method->Value";
                }
                else if (type == typeof(sbyte))
                {
                    return "(sbyte)ptr_of_this_method->Value";
                }
                else if (type == typeof(uint))
                {
                    return "(uint)ptr_of_this_method->Value";
                }
                else if (type == typeof(char))
                {
                    return "(char)ptr_of_this_method->Value";
                }
                else if (type == typeof(ulong))
                {
                    return "*(ulong*)&ptr_of_this_method->Value";
                }
                else
                    throw new NotImplementedException();
            }
            else
            {
                return string.Format("({0})typeof({0}).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags){1})", realClsName, (int)type.GetTypeFlagsRecursive());
            }
        }

        internal static void GetRefWriteBackValueCode(this Type type, StringBuilder sb, string paramName)
        {
            if (type.IsPrimitive)
            {
                if (type == typeof(int))
                {
                    sb.AppendLine("                        ___dst->ObjectType = ObjectTypes.Integer;");
                    sb.Append("                        ___dst->Value = @" + paramName);
                    sb.AppendLine(";");
                }
                else if (type == typeof(long))
                {
                    sb.AppendLine("                        ___dst->ObjectType = ObjectTypes.Long;");
                    sb.Append("                        *(long*)&___dst->Value = @" + paramName);
                    sb.AppendLine(";");
                }
                else if (type == typeof(short))
                {
                    sb.AppendLine("                        ___dst->ObjectType = ObjectTypes.Integer;");
                    sb.Append("                        ___dst->Value = @" + paramName);
                    sb.AppendLine(";");
                }
                else if (type == typeof(bool))
                {
                    sb.AppendLine("                        ___dst->ObjectType = ObjectTypes.Integer;");
                    sb.Append("                        ___dst->Value = @" + paramName + " ? 1 : 0;");
                    sb.AppendLine(";");
                }
                else if (type == typeof(ushort))
                {
                    sb.AppendLine("                        ___dst->ObjectType = ObjectTypes.Integer;");
                    sb.Append("                        ___dst->Value = @" + paramName);
                    sb.AppendLine(";");
                }
                else if (type == typeof(float))
                {
                    sb.AppendLine("                        ___dst->ObjectType = ObjectTypes.Float;");
                    sb.Append("                        *(float*)&___dst->Value = @" + paramName);
                    sb.AppendLine(";");
                }
                else if (type == typeof(double))
                {
                    sb.AppendLine("                        ___dst->ObjectType = ObjectTypes.Double;");
                    sb.Append("                        *(double*)&___dst->Value = @" + paramName);
                    sb.AppendLine(";");
                }
                else if (type == typeof(byte))
                {
                    sb.AppendLine("                        ___dst->ObjectType = ObjectTypes.Integer;");
                    sb.Append("                        ___dst->Value = @" + paramName);
                    sb.AppendLine(";");
                }
                else if (type == typeof(sbyte))
                {
                    sb.AppendLine("                        ___dst->ObjectType = ObjectTypes.Integer;");
                    sb.Append("                        ___dst->Value = @" + paramName);
                    sb.AppendLine(";");
                }
                else if (type == typeof(uint))
                {
                    sb.AppendLine("                        ___dst->ObjectType = ObjectTypes.Integer;");
                    sb.Append("                        ___dst->Value = (int)@" + paramName);
                    sb.AppendLine(";");
                }
                else if (type == typeof(char))
                {
                    sb.AppendLine("                        ___dst->ObjectType = ObjectTypes.Integer;");
                    sb.Append("                        ___dst->Value = (int)@" + paramName);
                    sb.AppendLine(";");
                }
                else if (type == typeof(ulong))
                {
                    sb.AppendLine("                        ___dst->ObjectType = ObjectTypes.Long;");
                    sb.Append("                        *(ulong*)&___dst->Value = @" + paramName);
                    sb.AppendLine(";");
                }
                else
                    throw new NotImplementedException();
            }
            else if(type.IsEnum)
            {
                sb.AppendLine("                        ___dst->ObjectType = ObjectTypes.Integer;");
                sb.Append("                        ___dst->Value = (int)@" + paramName);
                sb.AppendLine(";");
            }
            else
            {
                sb.Append(@"                        object ___obj = @");
                sb.Append(paramName);
                sb.AppendLine(";");
                sb.AppendLine(@"                        if (___dst->ObjectType >= ObjectTypes.Object)
                        {
                            if (___obj is CrossBindingAdaptorType)
                                ___obj = ((CrossBindingAdaptorType)___obj).ILInstance;
                            __mStack[___dst->Value] = ___obj;
                        }
                        else
                        {
                            ILIntepreter.UnboxObject(___dst, ___obj, __mStack, __domain);
                        }");
                /*if (!type.IsValueType)
                {
                    sb.Append(@"                        object ___obj = ");
                    sb.Append(paramName);
                    sb.AppendLine(";");

                    sb.AppendLine(@"                        if (___obj is CrossBindingAdaptorType)
                            ___obj = ((CrossBindingAdaptorType)___obj).ILInstance;
                        __mStack[___dst->Value] = ___obj; ");
                }
                else
                {
                    sb.Append("                        __mStack[___dst->Value] = ");
                    sb.Append(paramName);
                    sb.AppendLine(";");
                }*/
            }
        }

        internal static void GetReturnValueCode(this Type type, StringBuilder sb, Enviorment.AppDomain domain)
        {
            if (type.IsPrimitive)
            {
                if (type == typeof(int))
                {
                    sb.AppendLine("            __ret->ObjectType = ObjectTypes.Integer;");
                    sb.AppendLine("            __ret->Value = result_of_this_method;");
                }
                else if (type == typeof(long))
                {
                    sb.AppendLine("            __ret->ObjectType = ObjectTypes.Long;");
                    sb.AppendLine("            *(long*)&__ret->Value = result_of_this_method;");
                }
                else if (type == typeof(short))
                {
                    sb.AppendLine("            __ret->ObjectType = ObjectTypes.Integer;");
                    sb.AppendLine("            __ret->Value = result_of_this_method;");
                }
                else if (type == typeof(bool))
                {
                    sb.AppendLine("            __ret->ObjectType = ObjectTypes.Integer;");
                    sb.AppendLine("            __ret->Value = result_of_this_method ? 1 : 0;");
                }
                else if (type == typeof(ushort))
                {
                    sb.AppendLine("            __ret->ObjectType = ObjectTypes.Integer;");
                    sb.AppendLine("            __ret->Value = result_of_this_method;");
                }
                else if (type == typeof(float))
                {
                    sb.AppendLine("            __ret->ObjectType = ObjectTypes.Float;");
                    sb.AppendLine("            *(float*)&__ret->Value = result_of_this_method;");
                }
                else if (type == typeof(double))
                {
                    sb.AppendLine("            __ret->ObjectType = ObjectTypes.Double;");
                    sb.AppendLine("            *(double*)&__ret->Value = result_of_this_method;");
                }
                else if (type == typeof(byte))
                {
                    sb.AppendLine("            __ret->ObjectType = ObjectTypes.Integer;");
                    sb.AppendLine("            __ret->Value = result_of_this_method;");
                }
                else if (type == typeof(sbyte))
                {
                    sb.AppendLine("            __ret->ObjectType = ObjectTypes.Integer;");
                    sb.AppendLine("            __ret->Value = result_of_this_method;");
                }
                else if (type == typeof(uint))
                {
                    sb.AppendLine("            __ret->ObjectType = ObjectTypes.Integer;");
                    sb.AppendLine("            __ret->Value = (int)result_of_this_method;");
                }
                else if (type == typeof(char))
                {
                    sb.AppendLine("            __ret->ObjectType = ObjectTypes.Integer;");
                    sb.AppendLine("            __ret->Value = (int)result_of_this_method;");
                }
                else if (type == typeof(ulong))
                {
                    sb.AppendLine("            __ret->ObjectType = ObjectTypes.Long;");
                    sb.AppendLine("            *(ulong*)&__ret->Value = result_of_this_method;");
                }
                else
                    throw new NotImplementedException();
                sb.AppendLine("            return __ret + 1;");

            }
            else
            {
                string isBox;
                if (type == typeof(object))
                    isBox = ", true";
                else
                    isBox = "";
                if (!type.IsSealed && type != typeof(ILRuntime.Runtime.Intepreter.ILTypeInstance))
                {
                    if(domain == null || CheckAssignableToCrossBindingAdapters(domain, type))
                    {
                        sb.Append(@"            object obj_result_of_this_method = result_of_this_method;
            if(obj_result_of_this_method is CrossBindingAdaptorType)
            {    
                return ILIntepreter.PushObject(__ret, __mStack, ((CrossBindingAdaptorType)obj_result_of_this_method).ILInstance");
                        sb.Append(isBox);
                        sb.AppendLine(@");
            }");
                    }
                    else if (typeof(CrossBindingAdaptorType).IsAssignableFrom(type))
                    {
                        sb.AppendLine(string.Format("            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method.ILInstance{0});", isBox));
                        return;
                    }
                    
                }
                sb.AppendLine(string.Format("            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method{0});", isBox));
            }
        }

        static bool CheckAssignableToCrossBindingAdapters(Enviorment.AppDomain domain, Type type)
        {
            if (type == typeof(object))
                return true;
            bool res = domain.CrossBindingAdaptors.ContainsKey(type);
            if (!res)
            {
                var baseType = type.BaseType;
                if (baseType != null && baseType != typeof(object))
                {
                    res = CheckAssignableToCrossBindingAdapters(domain, baseType);
                }
            }
            if (!res)
            {
                var interfaces = type.GetInterfaces();
                foreach(var i in interfaces)
                {
                    res = CheckAssignableToCrossBindingAdapters(domain, i);
                    if (res)
                        break;
                }
            }
            return res;
        }

        internal static bool HasByRefParam(this ParameterInfo[] param)
        {
            for (int j = param.Length; j > 0; j--)
            {
                var p = param[j - 1];
                if (p.ParameterType.IsByRef)
                    return true;
            }
            return false;
        }
    }
}
