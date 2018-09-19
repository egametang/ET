using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Text;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.CLR.Utils;

namespace ILRuntime.Runtime.CLRBinding
{
    static class MethodBindingGenerator
    {
        internal static string GenerateMethodRegisterCode(this Type type, MethodInfo[] methods, HashSet<MethodBase> excludes, out bool needMethods)
        {
            needMethods = false;
            MethodInfo[] allMethods = type.GetMethods();

            StringBuilder sb = new StringBuilder();
            int idx = 0;
            bool isMethodsGot = false;
            foreach (var i in methods)
            {
                if (excludes != null && excludes.Contains(i))
                    continue;
                if (type.ShouldSkipMethod(i))
                    continue;

                if (i.IsGenericMethod)
                {
                    if (!isMethodsGot)
                    {
                        sb.AppendLine(@"            Dictionary<string, List<MethodInfo>> genericMethods = new Dictionary<string, List<MethodInfo>>();
            List<MethodInfo> lst = null;                    
            foreach(var m in type.GetMethods())
            {
                if(m.IsGenericMethodDefinition)
                {
                    if (!genericMethods.TryGetValue(m.Name, out lst))
                    {
                        lst = new List<MethodInfo>();
                        genericMethods[m.Name] = lst;
                    }
                    lst.Add(m);
                }
            }");
                        isMethodsGot = true;
                    }

                    var param = i.GetGenericArguments();
                    StringBuilder sb2 = new StringBuilder();
                    sb2.Append("{");
                    bool first = true;
                    foreach (var j in param)
                    {
                        if (first)
                            first = false;
                        else
                            sb2.Append(", ");
                        sb2.Append("typeof(");
                        string clsName, realClsName;
                        bool isByRef;
                        j.GetClassName(out clsName, out realClsName, out isByRef);
                        sb2.Append(realClsName);
                        sb2.Append(")");
                        if (isByRef)
                            sb2.Append(".MakeByRefType()");
                    }
                    sb2.Append("}");
                    sb.AppendLine(string.Format("            args = new Type[]{0};", sb2));
                    sb.AppendLine(string.Format("            if (genericMethods.TryGetValue(\"{0}\", out lst))", i.Name));
                    sb.Append(@"            {
                foreach(var m in lst)
                {
                    if(m.GetParameters().Length == ");
                    sb.Append(i.GetParameters().Length.ToString());
                    sb.Append(@")
                    {
                        method = m.MakeGenericMethod(args);
                        app.RegisterCLRMethodRedirection(method, ");
                    sb.AppendLine(string.Format("{0}_{1});", i.Name, idx));
                    sb.AppendLine(@"
                        break;
                    }
                }
            }");
                }
                else
                {
                    string clsName, realClsName;
                    bool isByRef;
                    var param = i.GetParameters();
                    StringBuilder sb2 = new StringBuilder();
                    sb2.Append("{");
                    bool first = true;
                    foreach (var j in param)
                    {
                        if (first)
                            first = false;
                        else
                            sb2.Append(", ");
                        sb2.Append("typeof(");
                        j.ParameterType.GetClassName(out clsName, out realClsName, out isByRef);
                        sb2.Append(realClsName);
                        sb2.Append(")");
                        if (isByRef)
                            sb2.Append(".MakeByRefType()");
                    }
                    sb2.Append("}");
                    sb.AppendLine(string.Format("            args = new Type[]{0};", sb2));

                    i.ReturnType.GetClassName(out clsName, out realClsName, out isByRef);
                    if ((i.Name.Equals("op_Implicit") || i.Name.Equals("op_Explicit")) && allMethods.Count(m => m.Name.Equals(i.Name)) > 1)
                    {
                        // Type conversions can have different return types
                        needMethods = true;
                        sb.AppendLine(string.Format("            method = methods.Where(t => t.Name.Equals(\"{0}\") && t.ReturnType == typeof({1}) && t.CheckMethodParams(args)).Single();", i.Name, realClsName));
                    }
                    else if (allMethods.Any(m => m.IsGenericMethod && m.Name.Equals(i.Name) && m.CheckMethodParams(param)))
                    {
                        // Check for a generic method with the same name and params
                        needMethods = true;
                        sb.AppendLine(string.Format("            method = methods.Where(t => t.Name.Equals(\"{0}\") && t.CheckMethodParams(args)).Single();", i.Name));
                    } else
                        sb.AppendLine(string.Format("            method = type.GetMethod(\"{0}\", flag, null, args, null);", i.Name));

                    sb.AppendLine(string.Format("            app.RegisterCLRMethodRedirection(method, {0}_{1});", i.Name, idx));
                }

                idx++;
            }
            return sb.ToString();
        }
    
        internal static string GenerateMethodWraperCode(this Type type, MethodInfo[] methods, string typeClsName, HashSet<MethodBase> excludes, List<Type> valueTypeBinders)
        {
            StringBuilder sb = new StringBuilder();
            bool isMultiArr = type.IsArray && type.GetArrayRank() > 1;
            int idx = 0;
            foreach (var i in methods)
            {
                if (excludes != null && excludes.Contains(i))
                    continue;
                if (type.ShouldSkipMethod(i))
                    continue;
                bool isProperty = i.IsSpecialName;
                var param = i.GetParameters();
                int paramCnt = param.Length;
                if (!i.IsStatic)
                    paramCnt++;
                sb.AppendLine(string.Format("        static StackObject* {0}_{1}(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)", i.Name, idx));
                sb.AppendLine("        {");
                sb.AppendLine("            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;");
                if (param.Length != 0 || !i.IsStatic)
                    sb.AppendLine("            StackObject* ptr_of_this_method;");
                sb.AppendLine(string.Format("            StackObject* __ret = ILIntepreter.Minus(__esp, {0});", paramCnt));
                sb.AppendLine();
                bool hasByRef = param.HasByRefParam();
                string shouldFreeParam = hasByRef ? "false" : "true";
                for (int j = param.Length; j > 0; j--)
                {
                    var p = param[j - 1];
                    sb.AppendLine(string.Format("            ptr_of_this_method = ILIntepreter.Minus(__esp, {0});", param.Length - j + 1));
                    string clsName, realClsName;
                    bool isByRef;
                    p.ParameterType.GetClassName(out clsName, out realClsName, out isByRef);
                    var pt = p.ParameterType.IsByRef ? p.ParameterType.GetElementType() : p.ParameterType;
                    if (pt.IsValueType && !pt.IsPrimitive && valueTypeBinders != null && valueTypeBinders.Contains(pt))
                    {
                        if (isMultiArr)
                            sb.AppendLine(string.Format("            {0} a{1} = new {0}();", realClsName, j));
                        else
                            sb.AppendLine(string.Format("            {0} @{1} = new {0}();", realClsName, p.Name));

                        sb.AppendLine(string.Format("            if (ILRuntime.Runtime.Generated.CLRBindings.s_{0}_Binder != null) {{", clsName));

                        if (isMultiArr)
                            sb.AppendLine(string.Format("                ILRuntime.Runtime.Generated.CLRBindings.s_{1}_Binder.ParseValue(ref a{0}, __intp, ptr_of_this_method, __mStack, {2});", j, clsName, shouldFreeParam));
                        else
                            sb.AppendLine(string.Format("                ILRuntime.Runtime.Generated.CLRBindings.s_{1}_Binder.ParseValue(ref @{0}, __intp, ptr_of_this_method, __mStack, {2});", p.Name, clsName, shouldFreeParam));

                        sb.AppendLine("            } else {");

                        if (isByRef)
                            sb.AppendLine("                ptr_of_this_method = ILIntepreter.GetObjectAndResolveReference(ptr_of_this_method);");
                        if (isMultiArr)
                            sb.AppendLine(string.Format("                a{0} = {1};", j, p.ParameterType.GetRetrieveValueCode(realClsName)));
                        else
                            sb.AppendLine(string.Format("                @{0} = {1};", p.Name, p.ParameterType.GetRetrieveValueCode(realClsName)));
                        if (!hasByRef)
                            sb.AppendLine("                __intp.Free(ptr_of_this_method);");

                        sb.AppendLine("            }");
                    }
                    else
                    {
                        if (isByRef)
                        {
                            if (p.ParameterType.GetElementType().IsPrimitive)
                            {
                                if (pt == typeof(int) || pt == typeof(uint) || pt == typeof(short) || pt == typeof(ushort) || pt == typeof(byte) || pt == typeof(sbyte) || pt == typeof(char))
                                {
                                    if (pt == typeof(int))
                                        sb.AppendLine(string.Format("            {0} @{1} = __intp.RetriveInt32(ptr_of_this_method, __mStack);", realClsName, p.Name));
                                    else
                                        sb.AppendLine(string.Format("            {0} @{1} = ({0})__intp.RetriveInt32(ptr_of_this_method, __mStack);", realClsName, p.Name));
                                }
                                else if (pt == typeof(long) || pt == typeof(ulong))
                                {
                                    if (pt == typeof(long))
                                        sb.AppendLine(string.Format("            {0} @{1} = __intp.RetriveInt64(ptr_of_this_method, __mStack);", realClsName, p.Name));
                                    else
                                        sb.AppendLine(string.Format("            {0} @{1} = ({0})__intp.RetriveInt64(ptr_of_this_method, __mStack);", realClsName, p.Name));
                                }
                                else if (pt == typeof(float))
                                {
                                    sb.AppendLine(string.Format("            {0} @{1} = __intp.RetriveFloat(ptr_of_this_method, __mStack);", realClsName, p.Name));
                                }
                                else if (pt == typeof(double))
                                {
                                    sb.AppendLine(string.Format("            {0} @{1} = __intp.RetriveDouble(ptr_of_this_method, __mStack);", realClsName, p.Name));
                                }
                                else if (pt == typeof(bool))
                                {
                                    sb.AppendLine(string.Format("            {0} @{1} = __intp.RetriveInt32(ptr_of_this_method, __mStack) == 1;", realClsName, p.Name));
                                }
                                else
                                    throw new NotSupportedException();
                            }
                            else
                            {
                                sb.AppendLine(string.Format("            {0} @{1} = ({0})typeof({0}).CheckCLRTypes(__intp.RetriveObject(ptr_of_this_method, __mStack));", realClsName, p.Name));
                            }

                        }
                        else
                        {
                            if (isMultiArr)
                                sb.AppendLine(string.Format("            {0} a{1} = {2};", realClsName, j, p.ParameterType.GetRetrieveValueCode(realClsName)));
                            else
                                sb.AppendLine(string.Format("            {0} @{1} = {2};", realClsName, p.Name, p.ParameterType.GetRetrieveValueCode(realClsName)));
                            if (!hasByRef && !p.ParameterType.IsPrimitive)
                                sb.AppendLine("            __intp.Free(ptr_of_this_method);");

                        }
                    }
                    sb.AppendLine();
                }
                if (!i.IsStatic)
                {
                    sb.AppendLine(string.Format("            ptr_of_this_method = ILIntepreter.Minus(__esp, {0});", paramCnt));
                    if (type.IsPrimitive)
                        sb.AppendLine(string.Format("            {0} instance_of_this_method = GetInstance(__domain, ptr_of_this_method, __mStack);", typeClsName));
                    else if (type.IsValueType && !type.IsPrimitive && valueTypeBinders != null && valueTypeBinders.Contains(type))
                    {
                        string clsName, realClsName;
                        bool isByRef;
                        type.GetClassName(out clsName, out realClsName, out isByRef);

                        sb.AppendLine(string.Format("            {0} instance_of_this_method = new {0}();", realClsName));

                        sb.AppendLine(string.Format("            if (ILRuntime.Runtime.Generated.CLRBindings.s_{0}_Binder != null) {{", clsName));

                        sb.AppendLine(string.Format("                ILRuntime.Runtime.Generated.CLRBindings.s_{0}_Binder.ParseValue(ref instance_of_this_method, __intp, ptr_of_this_method, __mStack, false);", clsName));

                        sb.AppendLine("            } else {");

                        if (type.IsValueType)
                            sb.AppendLine("                ptr_of_this_method = ILIntepreter.GetObjectAndResolveReference(ptr_of_this_method);");
                        sb.AppendLine(string.Format("                instance_of_this_method = {0};", type.GetRetrieveValueCode(typeClsName)));
                        if (!type.IsValueType && !hasByRef)
                            sb.AppendLine("                __intp.Free(ptr_of_this_method);");

                        sb.AppendLine("            }");
                    }
                    else
                    {
                        if (type.IsValueType)
                            sb.AppendLine("            ptr_of_this_method = ILIntepreter.GetObjectAndResolveReference(ptr_of_this_method);");
                        sb.AppendLine(string.Format("            {0} instance_of_this_method = {1};", typeClsName, type.GetRetrieveValueCode(typeClsName)));
                        if (!type.IsValueType && !hasByRef)
                            sb.AppendLine("            __intp.Free(ptr_of_this_method);");
                    }
                }
                sb.AppendLine();
                if (i.ReturnType != typeof(void))
                {
                    sb.Append("            var result_of_this_method = ");
                }
                else
                    sb.Append("            ");
                string genericArguments = "";
                if (i.IsGenericMethod)
                {
                    var p = i.GetGenericArguments();
                    StringBuilder sb2 = new StringBuilder();
                    bool first = true;
                    sb2.Append('<');
                    foreach (var j in p)
                    {
                        if (first)
                            first = false;
                        else
                            sb2.Append(", ");
                        string clsName, realClsName;
                        bool isByRef;
                        j.GetClassName(out clsName, out realClsName, out isByRef);
                        sb2.Append(realClsName);
                    }
                    sb2.Append('>');
                    genericArguments = sb2.ToString();
                }
                if (i.IsStatic)
                {
                    if (isProperty)
                    {
                        string[] t = new string[2];
                        int firstUnderlineIndex = i.Name.IndexOf("_");
                        t[0] = i.Name.Substring(0, firstUnderlineIndex);
                        t[1] = i.Name.Substring(firstUnderlineIndex + 1);
                        string propType = t[0];

                        if (propType == "get")
                        {
                            bool isIndexer = param.Length > 0;
                            if (isIndexer)
                            {
                                sb.AppendLine(string.Format("{1}[{0}];", param[0].Name, typeClsName));
                            }
                            else
                                sb.AppendLine(string.Format("{1}.{0};", t[1], typeClsName));
                        }
                        else if (propType == "set")
                        {
                            bool isIndexer = param.Length > 1;
                            if (isIndexer)
                            {
                                sb.AppendLine(string.Format("{2}[{0}] = {1};", param[0].Name, param[1].Name, typeClsName));
                            }
                            else
                                sb.AppendLine(string.Format("{2}.{0} = {1};", t[1], param[0].Name, typeClsName));
                        }
                        else if (propType == "op")
                        {
                            switch (t[1])
                            {
                                case "Equality":
                                    sb.AppendLine(string.Format("{0} == {1};", param[0].Name, param[1].Name));
                                    break;
                                case "Inequality":
                                    sb.AppendLine(string.Format("{0} != {1};", param[0].Name, param[1].Name));
                                    break;
                                case "Addition":
                                    sb.AppendLine(string.Format("{0} + {1};", param[0].Name, param[1].Name));
                                    break;
                                case "Subtraction":
                                    sb.AppendLine(string.Format("{0} - {1};", param[0].Name, param[1].Name));
                                    break;
                                case "Multiply":
                                    sb.AppendLine(string.Format("{0} * {1};", param[0].Name, param[1].Name));
                                    break;
                                case "Division":
                                    sb.AppendLine(string.Format("{0} / {1};", param[0].Name, param[1].Name));
                                    break;
                                case "GreaterThan":
                                    sb.AppendLine(string.Format("{0} > {1};", param[0].Name, param[1].Name));
                                    break;
                                case "GreaterThanOrEqual":
                                    sb.AppendLine(string.Format("{0} >= {1};", param[0].Name, param[1].Name));
                                    break;
                                case "LessThan":
                                    sb.AppendLine(string.Format("{0} < {1};", param[0].Name, param[1].Name));
                                    break;
                                case "LessThanOrEqual":
                                    sb.AppendLine(string.Format("{0} <= {1};", param[0].Name, param[1].Name));
                                    break;
                                case "UnaryNegation":
                                    sb.AppendLine(string.Format("-{0};", param[0].Name));
                                    break;
                                case "Modulus":
                                    sb.AppendLine(string.Format("{0} % {1};", param[0].Name, param[1].Name));
                                    break;
                                case "Implicit":
                                case "Explicit":
                                    {
                                        string clsName, realClsName;
                                        bool isByRef;
                                        i.ReturnType.GetClassName(out clsName, out realClsName, out isByRef);
                                        sb.AppendLine(string.Format("({1}){0};", param[0].Name, realClsName));
                                    }
                                    break;
                                default:
                                    throw new NotImplementedException(i.Name);
                            }
                        }
                        else
                            throw new NotImplementedException();
                    }
                    else
                    {
                        sb.Append(string.Format("{0}.{1}{2}(", typeClsName, i.Name, genericArguments));
                        param.AppendParameters(sb);
                        sb.AppendLine(");");
                    }
                }
                else
                {
                    if (isProperty)
                    {
                        string[] t = new string[2];
                        int firstUnderlineIndex = i.Name.IndexOf("_");
                        t[0] = i.Name.Substring(0, firstUnderlineIndex);
                        t[1] = i.Name.Substring(firstUnderlineIndex + 1);
                        string propType = t[0];

                        if (propType == "get")
                        {
                            bool isIndexer = param.Length > 0;
                            if (isIndexer)
                            {
                                sb.AppendLine(string.Format("instance_of_this_method[{0}];", param[0].Name));
                            }
                            else
                                sb.AppendLine(string.Format("instance_of_this_method.{0};", t[1]));
                        }
                        else if (propType == "set")
                        {
                            bool isIndexer = param.Length > 1;
                            if (isIndexer)
                            {
                                sb.AppendLine(string.Format("instance_of_this_method[{0}] = {1};", param[0].Name, param[1].Name));
                            }
                            else
                                sb.AppendLine(string.Format("instance_of_this_method.{0} = {1};", t[1], param[0].Name));
                        }
                        else
                            throw new NotImplementedException();
                    }
                    else if (isMultiArr)
                    {
                        if (i.Name == "Get")
                        {
                            sb.Append("instance_of_this_method[");
                            param.AppendParameters(sb, true);
                            sb.AppendLine("];");
                        }
                        else
                        {
                            sb.Append("instance_of_this_method[");
                            param.AppendParameters(sb, true, 1);
                            sb.Append("]");
                            sb.Append(" = a");
                            sb.Append(param.Length);
                            sb.AppendLine(";");
                        }
                    }
                    else
                    {
                        sb.Append(string.Format("instance_of_this_method.{0}{1}(", i.Name, genericArguments));
                        param.AppendParameters(sb);
                        sb.AppendLine(");");
                    }
                }
                sb.AppendLine();

                //Ref/Out
                for (int j = param.Length; j > 0; j--)
                {
                    var p = param[j - 1];
                    if (!p.ParameterType.IsByRef && !hasByRef)
                    {
                        continue;
                    }
                    string clsName, realClsName;
                    bool isByRef;
                    var pt = p.ParameterType.IsByRef ? p.ParameterType.GetElementType() : p.ParameterType;
                    pt.GetClassName(out clsName, out realClsName, out isByRef);
                    sb.AppendLine(string.Format("            ptr_of_this_method = ILIntepreter.Minus(__esp, {0});", param.Length - j + 1));
                    if (p.ParameterType.IsByRef)
                    {
                        sb.AppendLine(@"            switch(ptr_of_this_method->ObjectType)
            {
                case ObjectTypes.StackObjectReference:
                    {
                        var ___dst = *(StackObject**)&ptr_of_this_method->Value;");

                        if (pt.IsValueType && !pt.IsPrimitive && valueTypeBinders != null && valueTypeBinders.Contains(pt))
                        {
                            sb.AppendLine(string.Format("                if (ILRuntime.Runtime.Generated.CLRBindings.s_{0}_Binder != null) {{", clsName));

                            sb.AppendLine(string.Format("                        ILRuntime.Runtime.Generated.CLRBindings.s_{0}_Binder.WriteBackValue(__domain, ptr_of_this_method, __mStack, ref {1});", clsName, p.Name));

                            sb.AppendLine("                } else {");

                            pt.GetRefWriteBackValueCode(sb, p.Name);

                            sb.AppendLine("                }");
                        }
                        else
                        {
                            pt.GetRefWriteBackValueCode(sb, p.Name);
                        }

                        sb.Append(@"                    }
                    break;
                case ObjectTypes.FieldReference:
                    {
                        var ___obj = __mStack[ptr_of_this_method->Value];
                        if(___obj is ILTypeInstance)
                        {
                            ((ILTypeInstance)___obj)[ptr_of_this_method->ValueLow] = @");
                        sb.Append(p.Name);
                        sb.Append(@";
                        }
                        else
                        {
                            var ___type = __domain.GetType(___obj.GetType()) as CLRType;
                            ___type.SetFieldValue(ptr_of_this_method->ValueLow, ref ___obj, @");
                        sb.Append(p.Name);
                        sb.Append(@");
                        }
                    }
                    break;
                case ObjectTypes.StaticFieldReference:
                    {
                        var ___type = __domain.GetType(ptr_of_this_method->Value);
                        if(___type is ILType)
                        {
                            ((ILType)___type).StaticInstance[ptr_of_this_method->ValueLow] = @");
                        sb.Append(p.Name);
                        sb.Append(@";
                        }
                        else
                        {
                            ((CLRType)___type).SetStaticFieldValue(ptr_of_this_method->ValueLow, @");
                        sb.Append(p.Name);
                        sb.Append(@");
                        }
                    }
                    break;
                 case ObjectTypes.ArrayReference:
                    {
                        var instance_of_arrayReference = __mStack[ptr_of_this_method->Value] as ");
                        sb.Append(realClsName);
                        sb.Append(@"[];
                        instance_of_arrayReference[ptr_of_this_method->ValueLow] = @");
                        sb.Append(p.Name);
                        sb.AppendLine(@";
                    }
                    break;
            }");
                        sb.AppendLine();
                    }
                    else if(pt.IsValueType && !pt.IsPrimitive)
                    {
                        sb.AppendLine("            __intp.FreeStackValueType(ptr_of_this_method);");
                    }

                    sb.AppendLine("            __intp.Free(ptr_of_this_method);");
                }

                if (!i.IsStatic && ((type.IsValueType && !type.IsPrimitive) || hasByRef))//need to write back value type instance
                {
                    sb.AppendLine(string.Format("            ptr_of_this_method = ILIntepreter.Minus(__esp, {0});", paramCnt));

                    if (type.IsValueType && !type.IsPrimitive)
                    {
                        if (valueTypeBinders != null && valueTypeBinders.Contains(type))
                        {
                            string clsName, realClsName;
                            bool isByRef;
                            type.GetClassName(out clsName, out realClsName, out isByRef);

                            sb.AppendLine(string.Format("            if (ILRuntime.Runtime.Generated.CLRBindings.s_{0}_Binder != null) {{", clsName));

                            sb.AppendLine(string.Format("                ILRuntime.Runtime.Generated.CLRBindings.s_{0}_Binder.WriteBackValue(__domain, ptr_of_this_method, __mStack, ref instance_of_this_method);", clsName));

                            sb.AppendLine("            } else {");

                            sb.AppendLine("                WriteBackInstance(__domain, ptr_of_this_method, __mStack, ref instance_of_this_method);");

                            sb.AppendLine("            }");
                        }
                        else
                        {
                            sb.AppendLine("            WriteBackInstance(__domain, ptr_of_this_method, __mStack, ref instance_of_this_method);");
                        }
                        sb.AppendLine();
                    }
                    sb.AppendLine("            __intp.Free(ptr_of_this_method);");
                }
                
                if (i.ReturnType != typeof(void))
                {
                    if (i.ReturnType.IsValueType && !i.ReturnType.IsPrimitive && valueTypeBinders != null && valueTypeBinders.Contains(i.ReturnType))
                    {
                        string clsName, realClsName;
                        bool isByRef;
                        i.ReturnType.GetClassName(out clsName, out realClsName, out isByRef);

                        sb.AppendLine(string.Format("            if (ILRuntime.Runtime.Generated.CLRBindings.s_{0}_Binder != null) {{", clsName));

                        sb.AppendLine(string.Format("                ILRuntime.Runtime.Generated.CLRBindings.s_{0}_Binder.PushValue(ref result_of_this_method, __intp, __ret, __mStack);", clsName));
                        sb.AppendLine("                return __ret + 1;");

                        sb.AppendLine("            } else {");

                        i.ReturnType.GetReturnValueCode(sb);

                        sb.AppendLine("            }");
                    }
                    else
                    {
                        i.ReturnType.GetReturnValueCode(sb);
                    }
                }
                else
                    sb.AppendLine("            return __ret;");
                sb.AppendLine("        }");
                sb.AppendLine();
                idx++;
            }

            return sb.ToString();
        }
    }
}
