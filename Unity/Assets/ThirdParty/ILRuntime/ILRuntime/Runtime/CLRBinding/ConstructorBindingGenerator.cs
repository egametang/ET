using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Text;
using ILRuntime.Runtime.Enviorment;

namespace ILRuntime.Runtime.CLRBinding
{
    static class ConstructorBindingGenerator
    {
        internal static string GenerateConstructorRegisterCode(this Type type, ConstructorInfo[] methods, HashSet<MethodBase> excludes)
        {
            StringBuilder sb = new StringBuilder();
            int idx = 0;
            foreach (var i in methods)
            {
                if (excludes != null && excludes.Contains(i))
                    continue;
                if (type.ShouldSkipMethod(i))
                    continue;
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
                sb.AppendLine("            method = type.GetConstructor(flag, null, args, null);");
                sb.AppendLine(string.Format("            app.RegisterCLRMethodRedirection(method, Ctor_{0});",idx));

                idx++;
            }
            return sb.ToString();
        }

        internal static string GenerateConstructorWraperCode(this Type type, ConstructorInfo[] methods, string typeClsName, HashSet<MethodBase> excludes)
        {
            StringBuilder sb = new StringBuilder();

            int idx = 0;
            foreach (var i in methods)
            {
                if (excludes != null && excludes.Contains(i))
                    continue;
                if (type.ShouldSkipMethod(i) || i.IsStatic)
                    continue;
                var param = i.GetParameters();
                int paramCnt = param.Length;
                sb.AppendLine(string.Format("        static StackObject* Ctor_{0}(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)", idx));
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
                sb.AppendLine();
                sb.Append("            var result_of_this_method = ");
                {
                    string tmp, clsName;
                    bool isByRef;
                    type.GetClassName(out tmp, out clsName, out isByRef);
                    sb.Append(string.Format("new {0}(", clsName));
                    param.AppendParameters(sb);
                    sb.AppendLine(");");

                }
                sb.AppendLine();
                if (type.IsValueType)
                {
                    sb.AppendLine(@"            if(!isNewObj)
            {
                __ret--;
                WriteBackInstance(__domain, __ret, __mStack, ref result_of_this_method);
                return __ret;
            }");
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
                sb.AppendLine("            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);");

                sb.AppendLine("        }");
                sb.AppendLine();
                idx++;
            }

            return sb.ToString();
        }
    }
}
