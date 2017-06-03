using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Text;
using ILRuntime.Runtime.Enviorment;

namespace ILRuntime.Runtime.CLRBinding
{
    public class BindingCodeGenerator
    {
        public static void GenerateBindingCode(List<Type> types, string outputPath, HashSet<MethodBase> excludes = null)
        {
            if (!System.IO.Directory.Exists(outputPath))
                System.IO.Directory.CreateDirectory(outputPath);
            string[] oldFiles = System.IO.Directory.GetFiles(outputPath, "*.cs");
            foreach (var i in oldFiles)
            {
                System.IO.File.Delete(i);
            }
            List<string> clsNames = new List<string>();
            foreach (var i in types)
            {
                string clsName, realClsName;
                bool isByRef;
                if (i.GetCustomAttributes(typeof(ObsoleteAttribute), true).Length > 0)
                    continue;
                i.GetClassName(out clsName, out realClsName, out isByRef);
                clsNames.Add(clsName);
                using (System.IO.StreamWriter sw = new System.IO.StreamWriter(outputPath + "/" + clsName + ".cs", false, Encoding.UTF8))
                {
                    sw.Write(@"using System;
using System.Collections.Generic;
using System.Reflection;

using ILRuntime.CLR.TypeSystem;
using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;
using ILRuntime.Runtime.Stack;
using ILRuntime.Reflection;
using ILRuntime.CLR.Utils;

namespace ILRuntime.Runtime.Generated
{
    unsafe class ");
                    sw.WriteLine(clsName);
                    sw.Write(@"    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            MethodBase method;
            Type[] args;
            Type type = typeof(");
                    sw.Write(realClsName);
                    sw.WriteLine(");");
                    MethodInfo[] methods = i.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly);
                    string registerCode = GenerateRegisterCode(i, methods, excludes);
                    string commonCode = GenerateCommonCode(i, realClsName);
                    ConstructorInfo[] ctors = i.GetConstructors(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly);
                    string ctorRegisterCode = GenerateConstructorRegisterCode(i, ctors, excludes);
                    string wraperCode = GenerateWraperCode(i, methods, realClsName, excludes);
                    string ctorWraperCode = GenerateConstructorWraperCode(i, ctors, realClsName, excludes);
                    sw.WriteLine(registerCode);
                    sw.WriteLine(ctorRegisterCode);
                    sw.WriteLine("        }");
                    sw.WriteLine();
                    sw.WriteLine(commonCode);
                    sw.WriteLine(wraperCode);
                    sw.WriteLine(ctorWraperCode);
                    sw.WriteLine("    }");
                    sw.WriteLine("}");
                    sw.Flush();
                }
            }

            using (System.IO.StreamWriter sw = new System.IO.StreamWriter(outputPath + "/CLRBindings.cs", false, Encoding.UTF8))
            {
                sw.WriteLine(@"using System;
using System.Collections.Generic;
using System.Reflection;

namespace ILRuntime.Runtime.Generated
{
    class CLRBindings
    {
        /// <summary>
        /// Initialize the CLR binding, please invoke this AFTER CLR Redirection registration
        /// </summary>
        public static void Initialize(ILRuntime.Runtime.Enviorment.AppDomain app)
        {");
                foreach (var i in clsNames)
                {
                    sw.Write("            ");
                    sw.Write(i);
                    sw.WriteLine(".Register(app);");
                }

                sw.WriteLine(@"        }
    }
}");
            }
        }

        static bool ShouldSkipMethod(Type type, MethodBase i)
        {
            if (i.IsPrivate)
                return true;
            if (i.IsGenericMethod)
                return true;
            //EventHandler is currently not supported
            if (i.IsSpecialName)
            {
                string[] t = i.Name.Split('_');
                if (t[0] == "add" || t[0] == "remove")
                    return true;
                if (t[0] == "get" || t[0] == "set")
                {
                    var prop = type.GetProperty(t[1]);
                    if (prop == null)
                        return true;
                    if (prop.GetCustomAttributes(typeof(ObsoleteAttribute), true).Length > 0)
                        return true;
                }
            }
            if (i.GetCustomAttributes(typeof(ObsoleteAttribute), true).Length > 0)
                return true;
            var param = i.GetParameters();
            foreach(var j in param)
            {
                if (j.ParameterType.IsPointer)
                    return true;
            }
            return false;
        }

        static string GenerateRegisterCode(Type type, MethodInfo[] methods, HashSet<MethodBase> excludes)
        {
            StringBuilder sb = new StringBuilder();
            int idx = 0;
            foreach (var i in methods)
            {
                if (excludes != null && excludes.Contains(i))
                    continue;
                if (ShouldSkipMethod(type, i))
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

        static string GenerateConstructorRegisterCode(Type type, ConstructorInfo[] methods, HashSet<MethodBase> excludes)
        {
            StringBuilder sb = new StringBuilder();
            int idx = 0;
            foreach (var i in methods)
            {
                if (excludes != null && excludes.Contains(i))
                    continue;
                if (ShouldSkipMethod(type, i))
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

        static string GenerateCommonCode(Type type, string typeClsName)
        {
            if (!type.IsValueType)
                return "";
            StringBuilder sb = new StringBuilder();
            if (type.IsPrimitive)
            {
                sb.AppendLine(string.Format("        static {0} GetInstance(ILRuntime.Runtime.Enviorment.AppDomain __domain, StackObject* ptr_of_this_method, List<object> __mStack)", typeClsName));
                sb.AppendLine("        {");
                if (type.IsPrimitive || type.IsValueType)
                    sb.AppendLine("            ptr_of_this_method = ILIntepreter.GetObjectAndResolveReference(ptr_of_this_method);");
                sb.AppendLine(string.Format("            {0} instance_of_this_method;", typeClsName));
                sb.Append(@"            switch(ptr_of_this_method->ObjectType)
            {
                case ObjectTypes.FieldReference:
                    {
                        var instance_of_fieldReference = __mStack[ptr_of_this_method->Value];
                        if(instance_of_fieldReference is ILTypeInstance)
                        {
                            instance_of_this_method = (");
                sb.Append(typeClsName);
                sb.Append(")((ILTypeInstance)instance_of_fieldReference)[ptr_of_this_method->ValueLow];");
                sb.Append(@"
                        }
                        else
                        {
                            var t = __domain.GetType(instance_of_fieldReference.GetType()) as CLRType;
                            instance_of_this_method = (");
                sb.Append(typeClsName);
                sb.Append(")t.GetField(ptr_of_this_method->ValueLow).GetValue(instance_of_fieldReference);");
                sb.Append(@"
                        }
                    }
                    break;
                case ObjectTypes.StaticFieldReference:
                    {
                        var t = __domain.GetType(ptr_of_this_method->Value);
                        if(t is ILType)
                        {
                            instance_of_this_method = (");
                sb.Append(typeClsName);
                sb.Append(@")((ILType)t).StaticInstance[ptr_of_this_method->ValueLow];
                        }
                        else
                        {
                            instance_of_this_method = (");
                sb.Append(typeClsName);
                sb.Append(@")((CLRType)t).GetField(ptr_of_this_method->ValueLow).GetValue(null);
                        }
                    }
                    break;
                case ObjectTypes.ArrayReference:
                    {
                        var instance_of_arrayReference = __mStack[ptr_of_this_method->Value] as ");
                sb.Append(typeClsName);
                sb.AppendLine(@"[];
                        instance_of_this_method = instance_of_arrayReference[ptr_of_this_method->ValueLow];                        
                    }
                    break;
                default:");
                sb.AppendLine(string.Format("                    instance_of_this_method = {0};", GetRetrieveValueCode(type, typeClsName)));
                sb.AppendLine(@"                    break;
            }
            return instance_of_this_method;");
                sb.AppendLine("        }");
            }
            if (!type.IsPrimitive && !type.IsAbstract)
            {
                sb.AppendLine(string.Format("        static void WriteBackInstance(ILRuntime.Runtime.Enviorment.AppDomain __domain, StackObject* ptr_of_this_method, List<object> __mStack, ref {0} instance_of_this_method)", typeClsName));
                sb.AppendLine("        {");
                sb.AppendLine(@"            ptr_of_this_method = ILIntepreter.GetObjectAndResolveReference(ptr_of_this_method);
            switch(ptr_of_this_method->ObjectType)
            {
                case ObjectTypes.Object:
                    {
                        __mStack[ptr_of_this_method->Value] = instance_of_this_method;");
                sb.Append(@"                    }
                    break;
                case ObjectTypes.FieldReference:
                    {
                        var ___obj = __mStack[ptr_of_this_method->Value];
                        if(___obj is ILTypeInstance)
                        {
                            ((ILTypeInstance)___obj)[ptr_of_this_method->ValueLow] = instance_of_this_method");
                sb.Append(@";
                        }
                        else
                        {
                            var t = __domain.GetType(___obj.GetType()) as CLRType;
                            t.GetField(ptr_of_this_method->ValueLow).SetValue(___obj, instance_of_this_method");
                sb.Append(@");
                        }
                    }
                    break;
                case ObjectTypes.StaticFieldReference:
                    {
                        var t = __domain.GetType(ptr_of_this_method->Value);
                        if(t is ILType)
                        {
                            ((ILType)t).StaticInstance[ptr_of_this_method->ValueLow] = instance_of_this_method");
                sb.Append(@";
                        }
                        else
                        {
                            ((CLRType)t).GetField(ptr_of_this_method->ValueLow).SetValue(null, instance_of_this_method");
                sb.Append(@");
                        }
                    }
                    break;
                 case ObjectTypes.ArrayReference:
                    {
                        var instance_of_arrayReference = __mStack[ptr_of_this_method->Value] as ");
                sb.Append(typeClsName);
                sb.AppendLine(@"[];
                        instance_of_arrayReference[ptr_of_this_method->ValueLow] = instance_of_this_method;
                    }
                    break;
            }");
                sb.AppendLine(@"        }");
            }
            return sb.ToString();
        }

        static string GenerateConstructorWraperCode(Type type, ConstructorInfo[] methods, string typeClsName, HashSet<MethodBase> excludes)
        {
            StringBuilder sb = new StringBuilder();

            int idx = 0;
            foreach (var i in methods)
            {
                if (excludes != null && excludes.Contains(i))
                    continue;
                if (ShouldSkipMethod(type, i) || i.IsStatic)
                    continue;
                var param = i.GetParameters();
                int paramCnt = param.Length;
                sb.AppendLine(string.Format("        static StackObject* Ctor_{0}(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)", idx));
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
                    sb.AppendLine(string.Format("            {0} {1} = {2};", clsName, p.Name, GetRetrieveValueCode(p.ParameterType, clsName)));
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
                    AppendParameters(param, sb);
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
                    GetRefWriteBackValueCode(p.ParameterType.GetElementType(), sb, p.Name);
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
                            t.GetField(ptr_of_this_method->ValueLow).SetValue(___obj, ");
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
                            ((CLRType)t).GetField(ptr_of_this_method->ValueLow).SetValue(null, ");
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

        static string GenerateWraperCode(Type type, MethodInfo[] methods, string typeClsName, HashSet<MethodBase> excludes)
        {
            StringBuilder sb = new StringBuilder();

            int idx = 0;
            foreach (var i in methods)
            {
                if (excludes != null && excludes.Contains(i))
                    continue;
                if (ShouldSkipMethod(type, i))
                    continue;
                bool isProperty = i.IsSpecialName;
                var param = i.GetParameters();
                int paramCnt = param.Length;
                if (!i.IsStatic)
                    paramCnt++;
                sb.AppendLine(string.Format("        static StackObject* {0}_{1}(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)", i.Name, idx));
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
                    sb.AppendLine(string.Format("            {0} {1} = {2};", clsName, p.Name, GetRetrieveValueCode(p.ParameterType, clsName)));
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
                        sb.AppendLine(string.Format("            instance_of_this_method = {0};", GetRetrieveValueCode(type, typeClsName)));
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
                        AppendParameters(param, sb);
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
                        AppendParameters(param, sb);
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
                    GetRefWriteBackValueCode(p.ParameterType.GetElementType(), sb, p.Name);
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
                            t.GetField(ptr_of_this_method->ValueLow).SetValue(___obj, ");
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
                            ((CLRType)t).GetField(ptr_of_this_method->ValueLow).SetValue(null, ");
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
                    GetReturnValueCode(i.ReturnType, sb);
                }
                else
                    sb.AppendLine("            return __ret;");
                sb.AppendLine("        }");
                sb.AppendLine();
                idx++;
            }

            return sb.ToString();
        }

        static void AppendParameters(ParameterInfo[] param, StringBuilder sb)
        {
            bool first = true;
            foreach (var j in param)
            {
                if (first)
                    first = false;
                else
                    sb.Append(", ");
                if (j.IsOut)
                    sb.Append("out ");
                else if (j.ParameterType.IsByRef)
                    sb.Append("ref ");
                sb.Append(j.Name);
            }
        }

        static void GetRefWriteBackValueCode(Type type, StringBuilder sb, string paramName)
        {
            if (type.IsPrimitive)
            {
                if (type == typeof(int))
                {
                    sb.AppendLine("                        ___dst->ObjectType = ObjectTypes.Integer;");
                    sb.Append("                        ___dst->Value = " + paramName);
                    sb.AppendLine(";");
                }
                else if (type == typeof(long))
                {
                    sb.AppendLine("                        ___dst->ObjectType = ObjectTypes.Long;");
                    sb.Append("                        *(long*)&___dst->Value = " + paramName);
                    sb.AppendLine(";");
                }
                else if (type == typeof(short))
                {
                    sb.AppendLine("                        ___dst->ObjectType = ObjectTypes.Integer;");
                    sb.Append("                        ___dst->Value = " + paramName);
                    sb.AppendLine(";");
                }
                else if (type == typeof(bool))
                {
                    sb.AppendLine("                        ___dst->ObjectType = ObjectTypes.Integer;");
                    sb.Append("                        ___dst->Value = " + paramName + " ? 1 : 0;");
                    sb.AppendLine(";");
                }
                else if (type == typeof(ushort))
                {
                    sb.AppendLine("                        ___dst->ObjectType = ObjectTypes.Integer;");
                    sb.Append("                        ___dst->Value = " + paramName);
                    sb.AppendLine(";");
                }
                else if (type == typeof(float))
                {
                    sb.AppendLine("                        ___dst->ObjectType = ObjectTypes.Float;");
                    sb.Append("                        *(float*)&___dst->Value = " + paramName);
                    sb.AppendLine(";");
                }
                else if (type == typeof(double))
                {
                    sb.AppendLine("                        ___dst->ObjectType = ObjectTypes.Double;");
                    sb.Append("                        *(double*)&___dst->Value = " + paramName);
                    sb.AppendLine(";");
                }
                else if (type == typeof(byte))
                {
                    sb.AppendLine("                        ___dst->ObjectType = ObjectTypes.Integer;");
                    sb.Append("                        ___dst->Value = " + paramName);
                    sb.AppendLine(";");
                }
                else if (type == typeof(sbyte))
                {
                    sb.AppendLine("                        ___dst->ObjectType = ObjectTypes.Integer;");
                    sb.Append("                        ___dst->Value = " + paramName);
                    sb.AppendLine(";");
                }
                else if (type == typeof(uint))
                {
                    sb.AppendLine("                        ___dst->ObjectType = ObjectTypes.Integer;");
                    sb.Append("                        ___dst->Value = (int)" + paramName);
                    sb.AppendLine(";");
                }
                else if (type == typeof(char))
                {
                    sb.AppendLine("                        ___dst->ObjectType = ObjectTypes.Integer;");
                    sb.Append("                        ___dst->Value = (int)" + paramName);
                    sb.AppendLine(";");
                }
                else if (type == typeof(ulong))
                {
                    sb.AppendLine("                        ___dst->ObjectType = ObjectTypes.Long;");
                    sb.Append("                        *(ulong*)&___dst->Value = " + paramName);
                    sb.AppendLine(";");
                }
                else
                    throw new NotImplementedException();
            }
            else
            {
                if (!type.IsValueType)
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
                }
            }
        }

        static void GetReturnValueCode(Type type, StringBuilder sb)
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
                if (!type.IsSealed && type != typeof(ILRuntime.Runtime.Intepreter.ILTypeInstance))
                {
                    sb.AppendLine(@"            object obj_result_of_this_method = result_of_this_method;
            if(obj_result_of_this_method is CrossBindingAdaptorType)
            {    
                return ILIntepreter.PushObject(__ret, __mStack, ((CrossBindingAdaptorType)obj_result_of_this_method).ILInstance);
            }");
                }
                sb.AppendLine("            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);");
            }
        }

        static string GetRetrieveValueCode(Type type, string realClsName)
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
                return string.Format("({0})typeof({0}).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack))", realClsName);
            }
        }
    }
}
