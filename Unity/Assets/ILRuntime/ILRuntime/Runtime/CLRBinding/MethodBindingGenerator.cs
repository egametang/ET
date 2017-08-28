using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Text;
using ILRuntime.Runtime.Enviorment;

namespace ILRuntime.Runtime.CLRBinding
{
    static class MethodBindingGenerator
    {
        internal static string GenerateMethodRegisterCode(this Type type, MethodInfo[] methods, HashSet<MethodBase> excludes)
        {
            StringBuilder sb = new StringBuilder();
            int idx = 0;
            foreach (var i in methods)
            {
                if (excludes != null && excludes.Contains(i))
                    continue;
                if (type.ShouldSkipMethod(i))
                    continue;
                bool isProperty = i.IsSpecialName;
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
                    string tmp, clsName;
                    bool isByRef;
                    j.ParameterType.GetClassName(out tmp, out clsName, out isByRef);
                    sb2.Append(clsName);
                    sb2.Append(")");
                    if (isByRef)
                        sb2.Append(".MakeByRefType()");
                }
                sb2.Append("}");
                sb.AppendLine(string.Format("            args = new Type[]{0};", sb2));
                sb.AppendLine(string.Format("            method = type.GetMethod(\"{0}\", flag, null, args, null);", i.Name));
                sb.AppendLine(string.Format("            app.RegisterCLRMethodRedirection(method, {0}_{1});", i.Name, idx));

                idx++;
            }
            return sb.ToString();
        }

        internal static string GenerateMethodWraperCode(this Type type, MethodInfo[] methods, string typeClsName, HashSet<MethodBase> excludes)
        {
            StringBuilder sb = new StringBuilder();

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
                sb.AppendLine("            StackObject* ptr_of_this_method;");
                sb.AppendLine(string.Format("            StackObject* __ret = ILIntepreter.Minus(__esp, {0});", paramCnt));
                for (int j = param.Length; j > 0; j--)
                {
                    var p = param[j - 1];
                    sb.AppendLine(string.Format("            ptr_of_this_method = ILIntepreter.Minus(__esp, {0});", param.Length - j + 1));
                    string tmp, clsName;
                    bool isByRef;
                    p.ParameterType.GetClassName(out tmp, out clsName, out isByRef);
                    if (isByRef)
                        sb.AppendLine("            ptr_of_this_method = ILIntepreter.GetObjectAndResolveReference(ptr_of_this_method);");
                    sb.AppendLine(string.Format("            {0} {1} = {2};", clsName, p.Name, p.ParameterType.GetRetrieveValueCode(clsName)));
                    if (!isByRef && !p.ParameterType.IsPrimitive)
                        sb.AppendLine("            __intp.Free(ptr_of_this_method);");
                }
                if (!i.IsStatic)
                {
                    sb.AppendLine(string.Format("            ptr_of_this_method = ILIntepreter.Minus(__esp, {0});", paramCnt));
                    if (type.IsPrimitive)
                        sb.AppendLine(string.Format("            {0} instance_of_this_method = GetInstance(__domain, ptr_of_this_method, __mStack);", typeClsName));
                    else
                    {
                        if (type.IsValueType)
                            sb.AppendLine("            ptr_of_this_method = ILIntepreter.GetObjectAndResolveReference(ptr_of_this_method);");
                        sb.AppendLine(string.Format("            {0} instance_of_this_method;", typeClsName));
                        sb.AppendLine(string.Format("            instance_of_this_method = {0};", type.GetRetrieveValueCode(typeClsName)));
                        if (!type.IsValueType)
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
                if (i.IsStatic)
                {
                    if (isProperty)
                    {
                        string[] t = i.Name.Split('_');
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
                                case "Implicit":
                                case "Explicit":
                                    {
                                        string tmp, clsName;
                                        bool isByRef;
                                        i.ReturnType.GetClassName(out tmp, out clsName, out isByRef);
                                        sb.AppendLine(string.Format("({1}){0};", param[0].Name, clsName));
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
                        sb.Append(string.Format("{0}.{1}(", typeClsName, i.Name));
                        param.AppendParameters(sb);
                        sb.AppendLine(");");
                    }
                }
                else
                {
                    if (isProperty)
                    {
                        string[] t = i.Name.Split('_');
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
                    else
                    {
                        sb.Append(string.Format("instance_of_this_method.{0}(", i.Name));
                        param.AppendParameters(sb);
                        sb.AppendLine(");");
                    }
                }
                sb.AppendLine();


                if (!i.IsStatic && type.IsValueType && !type.IsPrimitive)//need to write back value type instance
                {
                    sb.AppendLine("            WriteBackInstance(__domain, ptr_of_this_method, __mStack, ref instance_of_this_method);");
                    sb.AppendLine();
                }
                //Ref/Out
                for (int j = param.Length; j > 0; j--)
                {
                    var p = param[j - 1];
                    if (!p.ParameterType.IsByRef)
                        continue;
                    string tmp, clsName;
                    bool isByRef;
                    p.ParameterType.GetElementType().GetClassName(out tmp, out clsName, out isByRef);
                    sb.AppendLine(string.Format("            ptr_of_this_method = ILIntepreter.Minus(__esp, {0});", param.Length - j + 1));
                    sb.AppendLine(@"            switch(ptr_of_this_method->ObjectType)
            {
                case ObjectTypes.StackObjectReference:
                    {
                        var ___dst = *(StackObject**)&ptr_of_this_method->Value;");
                    p.ParameterType.GetElementType().GetRefWriteBackValueCode(sb, p.Name);
                    sb.Append(@"                    }
                    break;
                case ObjectTypes.FieldReference:
                    {
                        var ___obj = __mStack[ptr_of_this_method->Value];
                        if(___obj is ILTypeInstance)
                        {
                            ((ILTypeInstance)___obj)[ptr_of_this_method->ValueLow] = ");
                    sb.Append(p.Name);
                    sb.Append(@";
                        }
                        else
                        {
                            var t = __domain.GetType(___obj.GetType()) as CLRType;
                            t.SetFieldValue(ptr_of_this_method->ValueLow, ref ___obj, ");
                    sb.Append(p.Name);
                    sb.Append(@");
                        }
                    }
                    break;
                case ObjectTypes.StaticFieldReference:
                    {
                        var t = __domain.GetType(ptr_of_this_method->Value);
                        if(t is ILType)
                        {
                            ((ILType)t).StaticInstance[ptr_of_this_method->ValueLow] = ");
                    sb.Append(p.Name);
                    sb.Append(@";
                        }
                        else
                        {
                            ((CLRType)t).SetStaticFieldValue(ptr_of_this_method->ValueLow, ");
                    sb.Append(p.Name);
                    sb.Append(@");
                        }
                    }
                    break;
                 case ObjectTypes.ArrayReference:
                    {
                        var instance_of_arrayReference = __mStack[ptr_of_this_method->Value] as ");
                    sb.Append(clsName);
                    sb.Append(@"[];
                        instance_of_arrayReference[ptr_of_this_method->ValueLow] = ");
                    sb.Append(p.Name);
                    sb.AppendLine(@";
                    }
                    break;
            }");
                    sb.AppendLine();
                }
                if (i.ReturnType != typeof(void))
                {
                    i.ReturnType.GetReturnValueCode(sb);
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
