using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ILRuntime.CLR.Method;
using ILRuntime.CLR.TypeSystem;
using ILRuntime.Runtime.Intepreter;
using System.Reflection;
using System.Threading;

namespace ILRuntime.Runtime.Enviorment
{
    public class CrossBindingCodeGenerator
    {
        class PropertyGenerateInfo
        {
            public string Name;
            public Type ReturnType;
            public string GetterBody;
            public string SettingBody;
            public string Modifier;
            public string OverrideString;
        }

        public static string GenerateCrossBindingAdapterCode(Type baseType, string nameSpace)
        {
            StringBuilder sb = new StringBuilder();
            List<MethodInfo> virtMethods = new List<MethodInfo>();
            GetMethods(baseType, virtMethods);
            string clsName, realClsName;
            bool isByRef;
            baseType.GetClassName(out clsName, out realClsName, out isByRef, true);
            sb.Append(@"using System;
using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;

namespace ");
            sb.AppendLine(nameSpace);
            sb.Append(@"{   
    public class ");
            sb.Append(clsName);
            sb.AppendLine(@"Adapter : CrossBindingAdaptor
    {");
            
            sb.Append(@"        public override Type BaseCLRType
        {
            get
            {
                return typeof(");
            sb.Append(realClsName);
            sb.AppendLine(@");
            }
        }

        public override Type AdaptorType
        {
            get
            {
                return typeof(Adapter);
            }
        }

        public override object CreateCLRInstance(ILRuntime.Runtime.Enviorment.AppDomain appdomain, ILTypeInstance instance)
        {
            return new Adapter(appdomain, instance);
        }
");

            sb.AppendLine(string.Format("        public class Adapter : {0}, CrossBindingAdaptorType", realClsName));
            sb.AppendLine("        {");
            GenerateCrossBindingMethodInfo(sb, virtMethods);
            sb.AppendLine(@"
            bool isInvokingToString;
            ILTypeInstance instance;
            ILRuntime.Runtime.Enviorment.AppDomain appdomain;

            public Adapter()
            {

            }

            public Adapter(ILRuntime.Runtime.Enviorment.AppDomain appdomain, ILTypeInstance instance)
            {
                this.appdomain = appdomain;
                this.instance = instance;
            }

            public ILTypeInstance ILInstance { get { return instance; } }
");
            GenerateCrossBindingMethodBody(sb, virtMethods);
            sb.Append(@"            public override string ToString()
            {
                IMethod m = appdomain.ObjectType.GetMethod("); sb.AppendLine("\"ToString\", 0);");
            sb.AppendLine(@"                m = instance.Type.GetVirtualMethod(m);
                if (m == null || m is ILMethod)
                {
                    if (!isInvokingToString)
                    {
                        isInvokingToString = true;
                        string res = instance.ToString();
                        isInvokingToString = false;
                        return res;
                    }
                    else
                        return instance.Type.FullName;
                }
                else
                    return instance.Type.FullName;");
            sb.AppendLine("            }");
            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine("}");
            return sb.ToString();
        }

        static void GenerateCrossBindingMethodBody(StringBuilder sb, List<MethodInfo> virtMethods)
        {
            int index = 0;
            Dictionary<string, PropertyGenerateInfo> pendingProperties = new Dictionary<string, PropertyGenerateInfo>();
            foreach (var i in virtMethods)
            {
                if (ShouldSkip(i))
                    continue;
                bool isProperty = i.IsSpecialName && (i.Name.StartsWith("get_") || i.Name.StartsWith("set_"));
                PropertyGenerateInfo pInfo = null;
                bool isGetter = false;
                bool isIndexFunc = false;
                bool isByRef;
                string clsName, realClsName;
                StringBuilder oriBuilder = null;

                if (isProperty)
                {
                    string pName = i.Name.Substring(4);
                    if (i.Name == "get_Item" || i.Name == "set_Item")
                    {
                        StringBuilder sBuilder = new StringBuilder();
                        var p = i.GetParameters()[0];
                        p.ParameterType.GetClassName(out clsName, out realClsName, out isByRef, true);
                        pName = $"this [{realClsName + " " + p.Name}]";

                        isIndexFunc = true;
                    }
                    isGetter = i.Name.StartsWith("get_");
                    oriBuilder = sb;
                    sb = new StringBuilder();
                    if (!pendingProperties.TryGetValue(pName, out pInfo))
                    {
                        pInfo = new PropertyGenerateInfo();
                        pInfo.Name = pName;
                        pendingProperties[pName] = pInfo;
                    }
                    if (pInfo.ReturnType == null)
                    {
                        if (isGetter)
                        {
                            pInfo.ReturnType = i.ReturnType;
                        }
                        else
                        {
                            pInfo.ReturnType = i.GetParameters()[0].ParameterType;
                        }
                    }
                }
                var param = i.GetParameters();
                string modifier = i.IsFamily ? "protected" : "public";
                string overrideStr = i.DeclaringType.IsInterface ? "" : (i.IsFinal ? "new " : "override ");
                string returnString = "";
                if (i.ReturnType != typeof(void))
                {
                    i.ReturnType.GetClassName(out clsName, out realClsName, out isByRef, true);
                    returnString = "return ";
                }
                else
                    realClsName = "void";
                if (!isProperty)
                {
                    sb.Append(string.Format("            {0} {3}{1} {2}(", modifier, realClsName, i.Name, overrideStr));
                    GetParameterDefinition(sb, param, true);
                    sb.AppendLine(@")
            {");
                }
                else
                {
                    pInfo.Modifier = modifier;
                    pInfo.OverrideString = overrideStr;
                }
                if (!i.IsAbstract)
                {
                    sb.AppendLine(string.Format("                if (m{0}_{1}.CheckShouldInvokeBase(this.instance))", i.Name, index));
                    if (isProperty)
                    {
                        string baseMethodName = isIndexFunc
                            ? $"base[{i.GetParameters()[0].Name}]"
                            : $"base.{i.Name.Substring(4)}";
                        if (isGetter)
                        {
                            sb.AppendLine(string.Format("                    return {0};", baseMethodName));
                        }
                        else
                        {
                            sb.AppendLine(string.Format("                    {0} = value;", baseMethodName));
                        }
                    }
                    else
                        sb.AppendLine(string.Format("                    {2}base.{0}({1});", i.Name, GetParameterName(param, true), returnString));
                    sb.AppendLine("                else");
                    sb.AppendLine(string.Format("                    {3}m{0}_{1}.Invoke(this.instance{2});", i.Name, index, GetParameterName(param, false), returnString));
                }
                else
                {
                    sb.AppendLine(string.Format("                {3}m{0}_{1}.Invoke(this.instance{2});", i.Name, index, GetParameterName(param, false), returnString));
                }
                if (isProperty)
                {
                    if (isGetter)
                    {
                        pInfo.GetterBody = sb.ToString();
                    }
                    else
                    {
                        pInfo.SettingBody = sb.ToString();
                    }
                    sb = oriBuilder;
                }
                else
                {
                    sb.AppendLine("            }");
                    sb.AppendLine();
                }
                index++;
            }

            foreach (var i in pendingProperties)
            {
                var pInfo = i.Value;
                string clsName, realClsName;
                bool isByRef;
                pInfo.ReturnType.GetClassName(out clsName, out realClsName, out isByRef, true);
                sb.AppendLine(string.Format("            {0} {3}{1} {2}", pInfo.Modifier, realClsName, pInfo.Name, pInfo.OverrideString));
                sb.AppendLine("            {");
                if (!string.IsNullOrEmpty(pInfo.GetterBody))
                {
                    sb.AppendLine("            get");
                    sb.AppendLine("            {");
                    sb.AppendLine(pInfo.GetterBody);
                    sb.AppendLine("            }");

                }
                if (!string.IsNullOrEmpty(pInfo.SettingBody))
                {
                    sb.AppendLine("            set");
                    sb.AppendLine("            {");
                    sb.AppendLine(pInfo.SettingBody);
                    sb.AppendLine("            }");

                }
                sb.AppendLine("            }");
                sb.AppendLine();
            }
        }

        static void GenerateCrossBindingMethodInfo(StringBuilder sb, List<MethodInfo> virtMethods)
        {
            int index = 0;
            foreach (var i in virtMethods)
            {
                if (ShouldSkip(i))
                    continue;
                var param = i.GetParameters();
                if (NeedGenerateCrossBindingMethodClass(param))
                {
                    GenerateCrossBindingMethodClass(sb, i.Name, index, param, i.ReturnType);
                    sb.AppendLine(string.Format("            {0}_{1}Info m{0}_{1} = new {0}_{1}Info();", i.Name, index));
                }
                else
                {
                    if (i.ReturnType != typeof(void))
                    {
                        sb.AppendLine(string.Format("            CrossBindingFunctionInfo<{0}> m{1}_{2} = new CrossBindingFunctionInfo<{0}>(\"{1}\");", GetParametersString(param, i.ReturnType), i.Name, index));
                    }
                    else
                    {
                        if (param.Length > 0)
                            sb.AppendLine(string.Format("            CrossBindingMethodInfo<{0}> m{1}_{2} = new CrossBindingMethodInfo<{0}>(\"{1}\");", GetParametersString(param, i.ReturnType), i.Name, index));
                        else
                            sb.AppendLine(string.Format("            CrossBindingMethodInfo m{0}_{1} = new CrossBindingMethodInfo(\"{0}\");", i.Name, index));
                    }
                }
                index++;
            }
        }

        static bool ShouldSkip(MethodInfo info)
        {
            var paramInfos = info.GetParameters();
            if (info.Name == "ToString" || info.Name == "GetHashCode" || info.Name == "Finalize")
                return paramInfos.Length == 0;
            if (info.Name == "Equals" && paramInfos.Length == 1 && paramInfos[0].ParameterType == typeof(object))
                return true;
            if (info.IsAssembly || info.IsFamilyOrAssembly || info.IsPrivate || info.IsFinal)
                return true;
            if (info.GetCustomAttributes(typeof(ObsoleteAttribute), true).Length > 0)
                return true;

            for (int i = 0; i < paramInfos.Length; ++i)
            {
                var paramType = paramInfos[i].ParameterType;
                if (paramType.IsByRef)
                    paramType = paramType.GetElementType();
                if (paramType.IsPointer || paramType.IsNotPublic || paramType.IsNested && !paramType.IsNestedPublic)
                {
                    return true;
                }
            }
            var returnType = info.ReturnType;
            if (returnType.IsByRef)
                returnType = returnType.GetElementType();
            if (returnType.IsNotPublic || returnType.IsNested && !returnType.IsNestedPublic)
                return true;

            return false;
        }
        static string GetParametersString(ParameterInfo[] param, Type returnType)
        {
            StringBuilder sb = new StringBuilder();
            bool first = true;
            foreach (var i in param)
            {
                if (!first)
                    sb.Append(", ");
                else
                    first = false;
                string clsName, realClsName;
                bool isByRef;
                i.ParameterType.GetClassName(out clsName, out realClsName, out isByRef, true);
                sb.Append(realClsName);
            }
            if (returnType != typeof(void))
            {
                if (!first)
                    sb.Append(", ");
                string clsName, realClsName;
                bool isByRef;
                returnType.GetClassName(out clsName, out realClsName, out isByRef, true);
                sb.Append(realClsName);
            }
            return sb.ToString();
        }

        static string GetParametersTypeString(ParameterInfo[] param, Type returnType)
        {
            StringBuilder sb = new StringBuilder();
            bool first = true;
            foreach (var i in param)
            {
                if (!first)
                    sb.Append(", ");
                else
                    first = false;
                string clsName, realClsName;
                bool isByRef;
                sb.Append("typeof(");
                i.ParameterType.GetClassName(out clsName, out realClsName, out isByRef, true);
                sb.Append(realClsName);
                sb.Append(")");
                if (isByRef)
                    sb.Append(".MakeByRefType()");
            }
            if (returnType != typeof(void))
            {
                if (!first)
                    sb.Append(", ");
                string clsName, realClsName;
                bool isByRef;
                returnType.GetClassName(out clsName, out realClsName, out isByRef, true);
                sb.Append(realClsName);
            }
            return sb.ToString();
        }

        static bool NeedGenerateCrossBindingMethodClass(ParameterInfo[] param)
        {
            if (param.Length > 5)
                return true;
            foreach (var i in param)
            {
                if (i.IsOut || i.ParameterType.IsByRef)
                    return true;
            }
            return false;
        }

        static string GetParameterName(ParameterInfo[] param, bool first)
        {
            StringBuilder sb = new StringBuilder();

            foreach (var p in param)
            {
                if (!first)
                    sb.Append(", ");
                else
                    first = false;
                if (p.IsOut)
                    sb.Append("out ");
                else if (p.ParameterType.IsByRef)
                    sb.Append("ref ");
                sb.Append(p.Name);
            }
            return sb.ToString();
        }

        static void GetParameterDefinition(StringBuilder sb, ParameterInfo[] param, bool first)
        {
            string clsName, realClsName;
            bool isByRef;

            foreach (var p in param)
            {
                if (!first)
                    sb.Append(", ");
                else
                    first = false;
                p.ParameterType.GetClassName(out clsName, out realClsName, out isByRef, true);
                if (p.IsOut)
                    sb.Append("out ");
                else if (isByRef)
                    sb.Append("ref ");
                sb.Append(realClsName);
                sb.Append(" ");
                sb.Append(p.Name);
            }
        }

        static void GenerateCrossBindingMethodClass(StringBuilder sb, string funcName, int index, ParameterInfo[] param, Type returnType)
        {
            sb.AppendLine(string.Format("            class {0}_{1}Info : CrossBindingMethodInfo", funcName, index));
            sb.Append(@"            {
                static Type[] pTypes = new Type[] {");
            sb.Append(GetParametersTypeString(param, returnType));
            sb.AppendLine("};");
            sb.AppendLine();
            sb.AppendLine(string.Format("                public {0}_{1}Info()", funcName, index));
            sb.AppendLine(string.Format("                    : base(\"{0}\")", funcName));
            sb.Append(@"                {

                }

                protected override Type ReturnType { get { return ");

            string clsName, realClsName;
            bool isByRef;
            returnType.GetClassName(out clsName, out realClsName, out isByRef, true);
            string rtRealName = realClsName;
            bool hasReturn = returnType != typeof(void);

            if (!hasReturn)
                sb.Append("null");
            else
            {
                sb.AppendFormat("typeof({0})", realClsName);
            }
            sb.AppendLine(@"; } }

                protected override Type[] Parameters { get { return pTypes; } }");
            sb.AppendFormat("                public {0} Invoke(ILTypeInstance instance", !hasReturn ? "void" : realClsName);
            GetParameterDefinition(sb, param, false);
            sb.AppendLine(@")
                {
                    EnsureMethod(instance);");
            GenInitParams(sb, param);
            sb.AppendLine(@"
                    if (method != null)
                    {
                        invoking = true;");

            if (hasReturn)
                sb.AppendLine(string.Format("                        {0} __res = default({0});", rtRealName));
            sb.AppendLine(@"                        try
                        {
                            using (var ctx = domain.BeginInvoke(method))
                            {");
            Dictionary<ParameterInfo, int> refIndex = new Dictionary<ParameterInfo, int>();
            int idx = 0;
            foreach (var p in param)
            {
                if (p.ParameterType.IsByRef)
                {
                    sb.AppendLine(GetPushString(p.ParameterType.GetElementType(), p.Name));
                    refIndex[p] = idx++;
                }
            }
            sb.AppendLine("                                ctx.PushObject(instance);");
            foreach (var p in param)
            {
                if (p.ParameterType.IsByRef)
                {
                    sb.AppendLine(string.Format("                                ctx.PushReference({0});", refIndex[p]));
                }
                else
                {
                    sb.AppendLine(GetPushString(p.ParameterType, p.Name));
                }
            }
            sb.AppendLine("                                ctx.Invoke();");
            if (hasReturn)
                sb.AppendLine(GetReadString(returnType, rtRealName, "", "__res"));
            foreach (var p in param)
            {
                if (p.ParameterType.IsByRef)
                {
                    p.ParameterType.GetClassName(out clsName, out realClsName, out isByRef, true);

                    sb.AppendLine(GetReadString(p.ParameterType.GetElementType(), realClsName, refIndex[p].ToString(), p.Name));
                }
            }
            sb.AppendLine("                            }");
            sb.AppendLine(@"                        }
                        finally
                        {
                            invoking = false;
                        }");
            if (hasReturn)
                sb.AppendLine("                       return __res;");
            sb.AppendLine("                    }");
            if (hasReturn)
                sb.AppendLine(@"                    else
                        return default(TResult);");
            sb.AppendLine(@"                }

                public override void Invoke(ILTypeInstance instance)
                {
                    throw new NotSupportedException();
                }
            }");
        }

        static void GetMethods(Type type, List<MethodInfo> list)
        {
            if (type == null)
                return;

            MethodInfo[] methods = type.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            foreach (var i in methods)
            {
                if ((i.IsVirtual || i.IsAbstract || type.IsInterface) && !i.ContainsGenericParameters)
                {
                    if (list.Any(m => IsMethodEqual(m, i)))
                        continue;

                    list.Add(i);
                }
            }

            var interfaceArray = type.GetInterfaces();
            if (interfaceArray != null)
            {
                for (int i = 0; i < interfaceArray.Length; ++i)
                {
                    GetMethods(interfaceArray[i], list);
                }
            }
        }

        static bool IsMethodEqual(MethodInfo left, MethodInfo right)
        {
            var leftParams = left.GetParameters();
            var rightParams = right.GetParameters();
            if (leftParams.Length != rightParams.Length)
            {
                return false;
            }

            // 有些继承了多个interface的类，且这几个interface的类有相同函数名的时候，子类实现的接口就会带上interfere的fullName
            string leftMethodName = left.Name.Replace(left.DeclaringType.FullName, "");
            string rightMethodName = right.Name.Replace(right.DeclaringType.FullName, "");
            if (leftMethodName != rightMethodName)
            {
                return false;
            }

            for (int i = 0; i < leftParams.Length; ++i)
            {
                if (leftParams[i].ParameterType != rightParams[i].ParameterType)
                {
                    return false;
                }
            }

            return true;
        }

        static void GenInitParams(StringBuilder sb, ParameterInfo[] param)
        {
            foreach (var p in param)
            {
                if (p.IsOut)
                {
                    sb.AppendLine($"                    {p.Name} = default({p.ParameterType.GetElementType().FullName});");
                }
            }
        }

        static string GetPushString(Type type, string argName)
        {
            if (type.IsPrimitive)
            {
                if (type == typeof(int))
                {
                    return string.Format("                            ctx.PushInteger({0});", argName);
                }
                else if (type == typeof(long))
                {
                    return string.Format("                            ctx.PushLong({0});", argName);
                }
                else if (type == typeof(short))
                {
                    return string.Format("                            ctx.PushInteger({0});", argName);
                }
                else if (type == typeof(bool))
                {
                    return string.Format("                            ctx.PushBool({0});", argName);
                }
                else if (type == typeof(ushort))
                {
                    return string.Format("                            ctx.PushInteger({0});", argName);
                }
                else if (type == typeof(float))
                {
                    return string.Format("                            ctx.PushFloat({0});", argName);
                }
                else if (type == typeof(double))
                {
                    return string.Format("                            ctx.PushDouble({0});", argName);
                }
                else if (type == typeof(byte))
                {
                    return string.Format("                            ctx.PushInteger({0});", argName);
                }
                else if (type == typeof(sbyte))
                {
                    return string.Format("                            ctx.PushInteger({0});", argName);
                }
                else if (type == typeof(uint))
                {
                    return string.Format("                            ctx.PushInteger((int){0});", argName);
                }
                else if (type == typeof(char))
                {
                    return string.Format("                            ctx.PushInteger((int){0});", argName);
                }
                else if (type == typeof(ulong))
                {
                    return string.Format("                            ctx.PushLong((long){0});", argName);
                }
                else
                    throw new NotImplementedException();
            }
            else
            {
                return string.Format("                            ctx.PushObject({0});", argName);
            }
        }

        static string GetReadString(Type type, string realClsName, string argName, string valName)
        {
            if (type.IsPrimitive)
            {
                if (type == typeof(int))
                {
                    return string.Format("                             {1} = ctx.ReadInteger({0});", argName, valName);
                }
                else if (type == typeof(long))
                {
                    return string.Format("                            {1} = ctx.ReadLong({0});", argName, valName);
                }
                else if (type == typeof(short))
                {
                    return string.Format("                            {1} = (short)ctx.ReadInteger({0});", argName, valName);
                }
                else if (type == typeof(bool))
                {
                    return string.Format("                            {1} = ctx.ReadBool({0});", argName, valName);
                }
                else if (type == typeof(ushort))
                {
                    return string.Format("                            {1} = (ushort)ctx.ReadInteger({0});", argName, valName);
                }
                else if (type == typeof(float))
                {
                    return string.Format("                            {1} = ctx.ReadFloat({0});", argName, valName);
                }
                else if (type == typeof(double))
                {
                    return string.Format("                            {1} = ctx.ReadDouble({0});", argName, valName);
                }
                else if (type == typeof(byte))
                {
                    return string.Format("                            {1} = (byte)ctx.ReadInteger({0});", argName, valName);
                }
                else if (type == typeof(sbyte))
                {
                    return string.Format("                            {1} = (sbyte)ctx.ReadInteger({0});", argName, valName);
                }
                else if (type == typeof(uint))
                {
                    return string.Format("                            {1} = (uint)ctx.ReadInteger({0});", argName, valName);
                }
                else if (type == typeof(char))
                {
                    return string.Format("                            {1} = (char)ctx.ReadInteger({0});", argName, valName);
                }
                else if (type == typeof(ulong))
                {
                    return string.Format("                            {1} = (ulong)ctx.ReadLong({0});", argName, valName);
                }
                else
                    throw new NotImplementedException();
            }
            else
            {
                return string.Format("                            {2} = ctx.ReadObject<{1}>({0});", argName, realClsName, valName);
            }
        }
    }
}
