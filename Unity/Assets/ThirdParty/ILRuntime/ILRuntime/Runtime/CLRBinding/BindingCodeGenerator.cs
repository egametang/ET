using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Text;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Other;

namespace ILRuntime.Runtime.CLRBinding
{
    public class BindingCodeGenerator
    {
        public static void GenerateBindingCode(List<Type> types, string outputPath, HashSet<MethodBase> excludeMethods = null, HashSet<FieldInfo> excludeFields = null)
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
using System.Runtime.InteropServices;

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
            FieldInfo field;
            Type[] args;
            Type type = typeof(");
                    sw.Write(realClsName);
                    sw.WriteLine(");");
                    MethodInfo[] methods = i.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly);
                    FieldInfo[] fields = i.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly);
                    string registerMethodCode = i.GenerateMethodRegisterCode(methods, excludeMethods);
                    string registerFieldCode = i.GenerateFieldRegisterCode(fields, excludeFields);
                    string registerValueTypeCode = i.GenerateValueTypeRegisterCode(realClsName);
                    string registerMiscCode = i.GenerateMiscRegisterCode(realClsName, true, true);
                    string commonCode = i.GenerateCommonCode(realClsName);
                    ConstructorInfo[] ctors = i.GetConstructors(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly);
                    string ctorRegisterCode = i.GenerateConstructorRegisterCode(ctors, excludeMethods);
                    string methodWraperCode = i.GenerateMethodWraperCode(methods, realClsName, excludeMethods);
                    string fieldWraperCode = i.GenerateFieldWraperCode(fields, realClsName, excludeFields);
                    string cloneWraperCode = i.GenerateCloneWraperCode(fields, realClsName);
                    string ctorWraperCode = i.GenerateConstructorWraperCode(ctors, realClsName, excludeMethods);
                    sw.WriteLine(registerMethodCode);
                    sw.WriteLine(registerFieldCode);
                    sw.WriteLine(registerValueTypeCode);
                    sw.WriteLine(registerMiscCode);
                    sw.WriteLine(ctorRegisterCode);
                    sw.WriteLine("        }");
                    sw.WriteLine();
                    sw.WriteLine(commonCode);
                    sw.WriteLine(methodWraperCode);
                    sw.WriteLine(fieldWraperCode);
                    sw.WriteLine(cloneWraperCode);
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

        class CLRBindingGenerateInfo
        {
            public Type Type { get; set; }
            public HashSet<MethodInfo> Methods { get; set; }
            public HashSet<FieldInfo> Fields { get; set; }
            public HashSet<ConstructorInfo> Constructors { get; set; }
            public bool ArrayNeeded { get; set; }
            public bool DefaultInstanceNeeded { get; set; }
            public bool ValueTypeNeeded { get; set; }

            public bool NeedGenerate
            {
                get
                {
                    if (Methods.Count == 0 && Constructors.Count == 0 && Fields.Count == 0 && !ArrayNeeded && !DefaultInstanceNeeded && !ValueTypeNeeded)
                        return false;
                    else
                    {
                        //Making CLRBinding for such types makes no sense
                        if (Type == typeof(Delegate) || Type == typeof(System.Runtime.CompilerServices.RuntimeHelpers))
                            return false;
                        return true;
                    }
                }
            }
        }

        public static void GenerateBindingCode(ILRuntime.Runtime.Enviorment.AppDomain domain, string outputPath)
        {
            if (domain == null)
                return;
            if (!System.IO.Directory.Exists(outputPath))
                System.IO.Directory.CreateDirectory(outputPath);
            Dictionary<Type, CLRBindingGenerateInfo> infos = new Dictionary<Type, CLRBindingGenerateInfo>(new ByReferenceKeyComparer<Type>());
            CrawlAppdomain(domain, infos);
            string[] oldFiles = System.IO.Directory.GetFiles(outputPath, "*.cs");
            foreach (var i in oldFiles)
            {
                System.IO.File.Delete(i);
            }

            HashSet<MethodBase> excludeMethods = null;
            HashSet<FieldInfo> excludeFields = null;
            HashSet<string> files = new HashSet<string>();
            List<string> clsNames = new List<string>();
            foreach (var info in infos)
            {
                if (!info.Value.NeedGenerate)
                    continue;
                Type i = info.Value.Type;
                if (i.BaseType == typeof(MulticastDelegate))
                    continue;
                string clsName, realClsName;
                bool isByRef;
                if (i.GetCustomAttributes(typeof(ObsoleteAttribute), true).Length > 0)
                    continue;
                i.GetClassName(out clsName, out realClsName, out isByRef);
                if (clsNames.Contains(clsName))
                    clsName = clsName + "_t";
                clsNames.Add(clsName);
                string oFileName = outputPath + "/" + clsName;
                int len = Math.Min(oFileName.Length, 100);
                if (len < oFileName.Length)
                    oFileName = oFileName.Substring(0, len) + "_t";
                while (files.Contains(oFileName))
                    oFileName = oFileName + "_t";
                files.Add(oFileName);
                oFileName = oFileName + ".cs";
                using (System.IO.StreamWriter sw = new System.IO.StreamWriter(oFileName, false, Encoding.UTF8))
                {
                    sw.Write(@"using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;

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
            FieldInfo field;
            Type[] args;
            Type type = typeof(");
                    sw.Write(realClsName);
                    sw.WriteLine(");");
                    MethodInfo[] methods = info.Value.Methods.ToArray();
                    FieldInfo[] fields = info.Value.Fields.ToArray();
                    string registerMethodCode = i.GenerateMethodRegisterCode(methods, excludeMethods);
                    string registerFieldCode = fields.Length > 0 ? i.GenerateFieldRegisterCode(fields, excludeFields) : null;
                    string registerValueTypeCode = info.Value.ValueTypeNeeded ? i.GenerateValueTypeRegisterCode(realClsName) : null;
                    string registerMiscCode = i.GenerateMiscRegisterCode(realClsName, info.Value.DefaultInstanceNeeded, info.Value.ArrayNeeded);
                    string commonCode = i.GenerateCommonCode(realClsName);
                    ConstructorInfo[] ctors = info.Value.Constructors.ToArray();
                    string ctorRegisterCode = i.GenerateConstructorRegisterCode(ctors, excludeMethods);
                    string methodWraperCode = i.GenerateMethodWraperCode(methods, realClsName, excludeMethods);
                    string fieldWraperCode = fields.Length > 0 ? i.GenerateFieldWraperCode(fields, realClsName, excludeFields) : null;
                    string cloneWraperCode = null;
                    if (info.Value.ValueTypeNeeded)
                    {
                        //Memberwise clone should copy all fields
                        var fs = i.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly);
                        cloneWraperCode = i.GenerateCloneWraperCode(fs, realClsName);
                    }
                    string ctorWraperCode = i.GenerateConstructorWraperCode(ctors, realClsName, excludeMethods);
                    sw.WriteLine(registerMethodCode);
                    if (fields.Length > 0)
                        sw.WriteLine(registerFieldCode);
                    if (info.Value.ValueTypeNeeded)
                        sw.WriteLine(registerValueTypeCode);
                    if (!string.IsNullOrEmpty(registerMiscCode))
                        sw.WriteLine(registerMiscCode);
                    sw.WriteLine(ctorRegisterCode);
                    sw.WriteLine("        }");
                    sw.WriteLine();
                    sw.WriteLine(commonCode);
                    sw.WriteLine(methodWraperCode);
                    if (fields.Length > 0)
                        sw.WriteLine(fieldWraperCode);
                    if (info.Value.ValueTypeNeeded)
                        sw.WriteLine(cloneWraperCode);
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

        static void CrawlAppdomain(ILRuntime.Runtime.Enviorment.AppDomain domain, Dictionary<Type, CLRBindingGenerateInfo> infos)
        {
            var arr = domain.LoadedTypes.Values.ToArray();
            //Prewarm
            foreach (var type in arr)
            {
                if (type is CLR.TypeSystem.ILType)
                {
                    if (type.HasGenericParameter)
                        continue;
                    var methods = type.GetMethods().ToList();
                    foreach (var i in ((CLR.TypeSystem.ILType)type).GetConstructors())
                        methods.Add(i);
                    if (((CLR.TypeSystem.ILType)type).GetStaticConstroctor() != null)
                        methods.Add(((CLR.TypeSystem.ILType)type).GetStaticConstroctor());
                    foreach (var j in methods)
                    {
                        CLR.Method.ILMethod method = j as CLR.Method.ILMethod;
                        if (method != null)
                        {
                            if (method.GenericParameterCount > 0 && !method.IsGenericInstance)
                                continue;
                            var body = method.Body;
                        }
                    }
                }
            }
            arr = domain.LoadedTypes.Values.ToArray();
            foreach (var type in arr)
            {
                if (type is CLR.TypeSystem.ILType)
                {
                    if (type.TypeForCLR.IsByRef || type.HasGenericParameter)
                        continue;
                    var methods = type.GetMethods().ToList();
                    foreach (var i in ((CLR.TypeSystem.ILType)type).GetConstructors())
                        methods.Add(i);

                    foreach (var j in methods)
                    {
                        CLR.Method.ILMethod method = j as CLR.Method.ILMethod;
                        if (method != null)
                        {
                            if (method.GenericParameterCount > 0 && !method.IsGenericInstance)
                                continue;
                            var body = method.Body;
                            foreach (var ins in body)
                            {
                                switch (ins.Code)
                                {
                                    case Intepreter.OpCodes.OpCodeEnum.Newobj:
                                        {
                                            CLR.Method.CLRMethod m = domain.GetMethod(ins.TokenInteger) as CLR.Method.CLRMethod;
                                            if (m != null)
                                            {
                                                if (m.DeclearingType.IsDelegate)
                                                    continue;
                                                Type t = m.DeclearingType.TypeForCLR;
                                                CLRBindingGenerateInfo info;
                                                if (!infos.TryGetValue(t, out info))
                                                {
                                                    info = CreateNewBindingInfo(t);
                                                    infos[t] = info;
                                                }
                                                if (m.IsConstructor)
                                                    info.Constructors.Add(m.ConstructorInfo);
                                                else
                                                    info.Methods.Add(m.MethodInfo);
                                            }
                                        }
                                        break;
                                    case Intepreter.OpCodes.OpCodeEnum.Ldfld:
                                    case Intepreter.OpCodes.OpCodeEnum.Stfld:
                                    case Intepreter.OpCodes.OpCodeEnum.Ldflda:
                                    case Intepreter.OpCodes.OpCodeEnum.Ldsfld:
                                    case Intepreter.OpCodes.OpCodeEnum.Ldsflda:
                                    case Intepreter.OpCodes.OpCodeEnum.Stsfld:
                                        {
                                            var t = domain.GetType((int)(ins.TokenLong >> 32)) as CLR.TypeSystem.CLRType;
                                            if(t != null)
                                            {
                                                var fi = t.GetField((int)ins.TokenLong);
                                                if (fi != null && fi.IsPublic)
                                                {
                                                    CLRBindingGenerateInfo info;
                                                    if (!infos.TryGetValue(t.TypeForCLR, out info))
                                                    {
                                                        info = CreateNewBindingInfo(t.TypeForCLR);
                                                        infos[t.TypeForCLR] = info;
                                                    }
                                                    if(ins.Code == Intepreter.OpCodes.OpCodeEnum.Stfld || ins.Code == Intepreter.OpCodes.OpCodeEnum.Stsfld)
                                                    {
                                                        if (t.IsValueType)
                                                        {
                                                            info.ValueTypeNeeded = true;
                                                            info.DefaultInstanceNeeded = true;
                                                        }
                                                    }
                                                    if (t.TypeForCLR.CheckCanPinn() || !t.IsValueType)
                                                        info.Fields.Add(fi);
                                                }
                                            }
                                        }
                                        break;
                                    case Intepreter.OpCodes.OpCodeEnum.Ldtoken:
                                        {
                                            if (ins.TokenInteger == 0)
                                            {
                                                var t = domain.GetType((int)(ins.TokenLong >> 32)) as CLR.TypeSystem.CLRType;
                                                if (t != null)
                                                {
                                                    var fi = t.GetField((int)ins.TokenLong);
                                                    if (fi != null)
                                                    {
                                                        CLRBindingGenerateInfo info;
                                                        if (!infos.TryGetValue(t.TypeForCLR, out info))
                                                        {
                                                            info = CreateNewBindingInfo(t.TypeForCLR);
                                                            infos[t.TypeForCLR] = info;
                                                        }
                                                        info.Fields.Add(fi);
                                                    }
                                                }
                                            }
                                        }
                                        break;
                                    case Intepreter.OpCodes.OpCodeEnum.Newarr:
                                        {
                                            var t = domain.GetType(ins.TokenInteger) as CLR.TypeSystem.CLRType;
                                            if(t != null)
                                            {
                                                CLRBindingGenerateInfo info;
                                                if (!infos.TryGetValue(t.TypeForCLR, out info))
                                                {
                                                    info = CreateNewBindingInfo(t.TypeForCLR);
                                                    infos[t.TypeForCLR] = info;
                                                }
                                                info.ArrayNeeded = true;
                                            }
                                        }
                                        break;
                                    case Intepreter.OpCodes.OpCodeEnum.Call:
                                    case Intepreter.OpCodes.OpCodeEnum.Callvirt:
                                        {
                                            CLR.Method.CLRMethod m = domain.GetMethod(ins.TokenInteger) as CLR.Method.CLRMethod;
                                            if (m != null)
                                            {
                                                //Cannot explicit call base class's constructor directly
                                                if (m.IsConstructor)
                                                    continue;
                                                if (!m.MethodInfo.IsPublic)
                                                    continue;
                                                Type t = m.DeclearingType.TypeForCLR;
                                                CLRBindingGenerateInfo info;
                                                if (!infos.TryGetValue(t, out info))
                                                {
                                                    info = CreateNewBindingInfo(t);
                                                    infos[t] = info;
                                                }

                                                info.Methods.Add(m.MethodInfo);
                                            }
                                        }
                                        break;
                                }
                            }
                        }
                    }
                }
            }
        }

        static CLRBindingGenerateInfo CreateNewBindingInfo(Type t)
        {
            CLRBindingGenerateInfo info = new CLRBindingGenerateInfo();
            info.Type = t;
            info.Methods = new HashSet<MethodInfo>();
            info.Fields = new HashSet<FieldInfo>();
            info.Constructors = new HashSet<ConstructorInfo>();
            if (t.IsValueType)
                info.DefaultInstanceNeeded = true;
            return info;
        }
    }
}
