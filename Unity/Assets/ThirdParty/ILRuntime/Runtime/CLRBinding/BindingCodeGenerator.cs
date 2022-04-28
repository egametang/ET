using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Other;

namespace ILRuntime.Runtime.CLRBinding
{
    public class BindingCodeGenerator
    {
        
        public static void GenerateBindingCode(List<Type> types, string outputPath, 
                                               HashSet<MethodBase> excludeMethods = null, HashSet<FieldInfo> excludeFields = null, 
                                               List<Type> valueTypeBinders = null, List<Type> delegateTypes = null)
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
                
                using (System.IO.StreamWriter sw = new System.IO.StreamWriter(outputPath + "/" + clsName + ".cs", false, new UTF8Encoding(false)))
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append(@"using System;
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
                    sb.AppendLine(clsName);
                    sb.Append(@"    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
");
                    string flagDef = "            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;";
                    string methodDef = "            MethodBase method;";
                    string methodsDef = "            MethodInfo[] methods = type.GetMethods(flag).Where(t => !t.IsGenericMethod).ToArray();";
                    string fieldDef = "            FieldInfo field;";
                    string argsDef = "            Type[] args;";
                    string typeDef = string.Format("            Type type = typeof({0});", realClsName);

                    bool needMethods;
                    MethodInfo[] methods = i.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly);
                    FieldInfo[] fields = i.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly);
                    string registerMethodCode = i.GenerateMethodRegisterCode(methods, excludeMethods, out needMethods);
                    string registerFieldCode = i.GenerateFieldRegisterCode(fields, excludeFields);
                    string registerValueTypeCode = i.GenerateValueTypeRegisterCode(realClsName);
                    string registerMiscCode = i.GenerateMiscRegisterCode(realClsName, true, true);
                    string commonCode = i.GenerateCommonCode(realClsName);
                    ConstructorInfo[] ctors = i.GetConstructors(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly);
                    string ctorRegisterCode = i.GenerateConstructorRegisterCode(ctors, excludeMethods);
                    string methodWraperCode = i.GenerateMethodWraperCode(methods, realClsName, excludeMethods, valueTypeBinders, null);
                    string fieldWraperCode = i.GenerateFieldWraperCode(fields, realClsName, excludeFields, valueTypeBinders, null);
                    string cloneWraperCode = i.GenerateCloneWraperCode(fields, realClsName);
                    string ctorWraperCode = i.GenerateConstructorWraperCode(ctors, realClsName, excludeMethods, valueTypeBinders);

                    bool hasMethodCode = !string.IsNullOrEmpty(registerMethodCode);
                    bool hasFieldCode = !string.IsNullOrEmpty(registerFieldCode);
                    bool hasValueTypeCode = !string.IsNullOrEmpty(registerValueTypeCode);
                    bool hasMiscCode = !string.IsNullOrEmpty(registerMiscCode);
                    bool hasCtorCode = !string.IsNullOrEmpty(ctorRegisterCode);
                    bool hasNormalMethod = methods.Where(x => !x.IsGenericMethod).Count() != 0;

                    if ((hasMethodCode && hasNormalMethod) || hasFieldCode || hasCtorCode)
                        sb.AppendLine(flagDef);
                    if (hasMethodCode || hasCtorCode)
                        sb.AppendLine(methodDef);
                    if (hasFieldCode)
                        sb.AppendLine(fieldDef);
                    if (hasMethodCode || hasFieldCode || hasCtorCode)
                        sb.AppendLine(argsDef);
                    if (hasMethodCode || hasFieldCode || hasValueTypeCode || hasMiscCode || hasCtorCode)
                        sb.AppendLine(typeDef);
                    if (needMethods)
                        sb.AppendLine(methodsDef);


                    sb.AppendLine(registerMethodCode);
                    sb.AppendLine(registerFieldCode);
                    sb.AppendLine(registerValueTypeCode);
                    sb.AppendLine(registerMiscCode);
                    sb.AppendLine(ctorRegisterCode);
                    sb.AppendLine("        }");
                    sb.AppendLine();
                    sb.AppendLine(commonCode);
                    sb.AppendLine(methodWraperCode);
                    sb.AppendLine(fieldWraperCode);
                    sb.AppendLine(cloneWraperCode);
                    sb.AppendLine(ctorWraperCode);
                    sb.AppendLine("    }");
                    sb.AppendLine("}");

                    sw.Write(Regex.Replace(sb.ToString(), "(?<!\r)\n", "\r\n"));
                    sw.Flush();
                }
            }

            var delegateClsNames = GenerateDelegateBinding(delegateTypes, outputPath);
            clsNames.AddRange(delegateClsNames);

            GenerateBindingInitializeScript(clsNames, valueTypeBinders, outputPath);
        }

        internal class CLRBindingGenerateInfo
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
        
        internal class FileNameEqualityComparer : IEqualityComparer<string>
        {
            public bool Equals(string x, string y)
            {
                // handle null cases first
                if (x == null)
                    return (y == null);
                // x != null
                else if (y == null)
                    return false;
                return x.Equals(y, StringComparison.OrdinalIgnoreCase);
            }

            public int GetHashCode(string obj)
            {
                int hashCode = 0;
                if (obj != null)
                {
                    hashCode = obj.GetHashCode();
                }
                return hashCode;
            }
        }

        public static void GenerateBindingCode(ILRuntime.Runtime.Enviorment.AppDomain domain, string outputPath, 
                                               List<Type> valueTypeBinders = null, List<Type> delegateTypes = null, int maxGenericDepth = 10,
                                               params string[] excludeFiles)
        {
            if (domain == null)
                return;
            if (!System.IO.Directory.Exists(outputPath))
                System.IO.Directory.CreateDirectory(outputPath);
            Dictionary<Type, CLRBindingGenerateInfo> infos = new Dictionary<Type, CLRBindingGenerateInfo>(new ByReferenceKeyComparer<Type>());
            CrawlAppdomain(domain, infos, maxGenericDepth);
            string[] oldFiles = System.IO.Directory.GetFiles(outputPath, "*.cs");
            foreach (var i in oldFiles)
            {
                System.IO.File.Delete(i);
            }

            if (valueTypeBinders == null)
                valueTypeBinders = new List<Type>(domain.ValueTypeBinders.Keys);

            HashSet<MethodBase> excludeMethods = null;
            HashSet<FieldInfo> excludeFields = null;
            HashSet<string> files = new HashSet<string>();
            List<string> clsNames = new List<string>();
            FileNameEqualityComparer fileNameEqualityComparer = new FileNameEqualityComparer();

            foreach (var info in infos)
            {
                if (!info.Value.NeedGenerate)
                    continue;
                Type i = info.Value.Type;

                //CLR binding for delegate is important for cross domain invocation,so it should be generated
                //if (i.BaseType == typeof(MulticastDelegate))
                //    continue;

                string clsName, realClsName;
                bool isByRef;
                if (i.GetCustomAttributes(typeof(ObsoleteAttribute), true).Length > 0)
                    continue;
                i.GetClassName(out clsName, out realClsName, out isByRef);
                if (excludeFiles.Contains(clsName))
                    continue;
                int extraClsNameIndex = 0;
                string oClsName = clsName;
                while (clsNames.Contains(oClsName))
                {
                    extraClsNameIndex++;
                    oClsName = clsName + "_t" + extraClsNameIndex;
                }
                clsNames.Add(oClsName);
                clsName = oClsName;
                
                //File path length limit
                string oriFileName = outputPath + "/" + clsName;
                int len = Math.Min(oriFileName.Length, 100);
                if (len < oriFileName.Length)
                    oriFileName = oriFileName.Substring(0, len);

                int extraNameIndex = 0;
                string oFileName = oriFileName;
                while (files.Contains(oFileName, fileNameEqualityComparer))
                {
                    extraNameIndex++;
                    oFileName = oriFileName + "_t" + extraNameIndex;
                }

                files.Add(oFileName);
                oFileName = oFileName + ".cs";
                using (System.IO.StreamWriter sw = new System.IO.StreamWriter(oFileName, false, new UTF8Encoding(false)))
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append(@"using System;
using System.Collections.Generic;
using System.Linq;
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
                    sb.AppendLine(clsName);
                    sb.Append(@"    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
");
                    string flagDef =    "            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;";
                    string methodDef =  "            MethodBase method;";
                    string methodsDef = "            MethodInfo[] methods = type.GetMethods(flag).Where(t => !t.IsGenericMethod).ToArray();";
                    string fieldDef =   "            FieldInfo field;";
                    string argsDef =    "            Type[] args;";
                    string typeDef = string.Format("            Type type = typeof({0});", realClsName);

                    bool needMethods;
                    MethodInfo[] methods = info.Value.Methods.ToArray();
                    FieldInfo[] fields = info.Value.Fields.ToArray();
                    string registerMethodCode = i.GenerateMethodRegisterCode(methods, excludeMethods, out needMethods);
                    string registerFieldCode = fields.Length > 0 ? i.GenerateFieldRegisterCode(fields, excludeFields) : null;
                    string registerValueTypeCode = info.Value.ValueTypeNeeded ? i.GenerateValueTypeRegisterCode(realClsName) : null;
                    string registerMiscCode = i.GenerateMiscRegisterCode(realClsName, info.Value.DefaultInstanceNeeded, info.Value.ArrayNeeded);
                    string commonCode = i.GenerateCommonCode(realClsName);
                    ConstructorInfo[] ctors = info.Value.Constructors.ToArray();
                    string ctorRegisterCode = i.GenerateConstructorRegisterCode(ctors, excludeMethods);
                    string methodWraperCode = i.GenerateMethodWraperCode(methods, realClsName, excludeMethods, valueTypeBinders, domain);
                    string fieldWraperCode = fields.Length > 0 ? i.GenerateFieldWraperCode(fields, realClsName, excludeFields, valueTypeBinders, domain) : null;
                    string cloneWraperCode = null;
                    if (info.Value.ValueTypeNeeded)
                    {
                        //Memberwise clone should copy all fields
                        var fs = i.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly);
                        cloneWraperCode = i.GenerateCloneWraperCode(fs, realClsName);
                    }

                    bool hasMethodCode = !string.IsNullOrEmpty(registerMethodCode);
                    bool hasFieldCode = !string.IsNullOrEmpty(registerFieldCode);
                    bool hasValueTypeCode = !string.IsNullOrEmpty(registerValueTypeCode);
                    bool hasMiscCode = !string.IsNullOrEmpty(registerMiscCode);
                    bool hasCtorCode = !string.IsNullOrEmpty(ctorRegisterCode);
                    bool hasNormalMethod = methods.Where(x => !x.IsGenericMethod).Count() != 0;

                    if ((hasMethodCode && hasNormalMethod) || hasFieldCode || hasCtorCode)
                        sb.AppendLine(flagDef);
                    if (hasMethodCode || hasCtorCode)
                        sb.AppendLine(methodDef);
                    if (hasFieldCode)
                        sb.AppendLine(fieldDef);
                    if (hasMethodCode || hasFieldCode || hasCtorCode)
                        sb.AppendLine(argsDef);
                    if (hasMethodCode || hasFieldCode || hasValueTypeCode || hasMiscCode || hasCtorCode)
                        sb.AppendLine(typeDef);
                    if (needMethods)
                        sb.AppendLine(methodsDef);

                    sb.AppendLine(registerMethodCode);
                    if (fields.Length > 0)
                        sb.AppendLine(registerFieldCode);
                    if (info.Value.ValueTypeNeeded)
                        sb.AppendLine(registerValueTypeCode);
                    if (!string.IsNullOrEmpty(registerMiscCode))
                        sb.AppendLine(registerMiscCode);
                    sb.AppendLine(ctorRegisterCode);
                    sb.AppendLine("        }");
                    sb.AppendLine();
                    sb.AppendLine(commonCode);
                    sb.AppendLine(methodWraperCode);
                    if (fields.Length > 0)
                        sb.AppendLine(fieldWraperCode);
                    if (info.Value.ValueTypeNeeded)
                        sb.AppendLine(cloneWraperCode);
                    string ctorWraperCode = i.GenerateConstructorWraperCode(ctors, realClsName, excludeMethods, valueTypeBinders);
                    sb.AppendLine(ctorWraperCode);
                    sb.AppendLine("    }");
                    sb.AppendLine("}");

                    sw.Write(Regex.Replace(sb.ToString(), "(?<!\r)\n", "\r\n"));
                    sw.Flush();
                }
            }

            using (System.IO.StreamWriter sw = new System.IO.StreamWriter(outputPath + "/CLRBindings.cs", false, new UTF8Encoding(false)))
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(@"using System;
using System.Collections.Generic;
using System.Reflection;

namespace ILRuntime.Runtime.Generated
{
    class CLRBindings
    {");
                sb.Append(SmartBindText);
                sb.Append(@"
        /// <summary>
        /// Initialize the CLR binding, please invoke this AFTER CLR Redirection registration
        /// </summary>
                public static void Initialize(ILRuntime.Runtime.Enviorment.AppDomain app)
        {");
                foreach (var i in clsNames)
                {
                    sb.Append("            ");
                    sb.Append(i);
                    sb.AppendLine(".Register(app);");
                }

                sb.AppendLine(@"        }
    }
}");
                sw.Write(Regex.Replace(sb.ToString(), "(?<!\r)\n", "\r\n"));
            }

            var delegateClsNames = GenerateDelegateBinding(delegateTypes, outputPath);
            clsNames.AddRange(delegateClsNames);

            GenerateBindingInitializeScript(clsNames, valueTypeBinders, outputPath);
        }

        static void PrewarmDomain(ILRuntime.Runtime.Enviorment.AppDomain domain)
        {
            var arr = domain.LoadedTypes.Values.ToArray();
            //Prewarm
            foreach (var type in arr)
            {
                if (type is CLR.TypeSystem.ILType)
                {
                    if (type.HasGenericParameter)
                    {
                        CLR.TypeSystem.ILType iltype = (CLR.TypeSystem.ILType)type;
                        if (iltype.GenericInstances != null)
                        {
                            foreach(var i in iltype.GenericInstances)
                            {
                                PrewarmType(i);
                            }
                        }
                    }
                    else
                        PrewarmType((CLR.TypeSystem.ILType)type);
                }
            }
        }

        static void PrewarmType(CLR.TypeSystem.ILType type)
        {
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
        internal static void CrawlAppdomain(ILRuntime.Runtime.Enviorment.AppDomain domain, Dictionary<Type, CLRBindingGenerateInfo> infos, int maxGenericDepth)
        {
            domain.SuppressStaticConstructor = true;
            for(int i = 0; i < maxGenericDepth; i++)
            {
                //Prewarm 10 times to ensure GenericMethods are prewarmed properly
                PrewarmDomain(domain);
            }
            var arr = domain.LoadedTypes.Values.ToArray();
            foreach (var type in arr)
            {
                if (type is CLR.TypeSystem.ILType)
                {
                    if (type.TypeForCLR.IsByRef)
                        continue;
                    if (type.HasGenericParameter)
                    {
                        CLR.TypeSystem.ILType iltype = (CLR.TypeSystem.ILType)type;
                        if (iltype.GenericInstances != null)
                        {
                            foreach (var i in iltype.GenericInstances)
                            {
                                CrawlType(domain, i, infos);
                            }
                        }
                    }
                    else
                        CrawlType(domain, (CLR.TypeSystem.ILType)type, infos);
                }
            }
        }

        static void CrawlType(ILRuntime.Runtime.Enviorment.AppDomain domain, CLR.TypeSystem.ILType type, Dictionary<Type, CLRBindingGenerateInfo> infos)
        {
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
                                    if (t != null)
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
                                            if (ins.Code == Intepreter.OpCodes.OpCodeEnum.Stfld || ins.Code == Intepreter.OpCodes.OpCodeEnum.Stsfld)
                                            {
                                                if (t.IsValueType)
                                                {
                                                    info.ValueTypeNeeded = true;
                                                    info.DefaultInstanceNeeded = true;
                                                }
                                            }
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
                                    if (t != null)
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
                                        if (m.IsConstructor && m.DeclearingType.CanAssignTo(((CLR.TypeSystem.ILType)type).FirstCLRBaseType))
                                            continue;
                                        if (m.IsConstructor)
                                        {
                                            if (!m.ConstructorInfo.IsPublic)
                                                continue;
                                            Type t = m.DeclearingType.TypeForCLR;
                                            CLRBindingGenerateInfo info;
                                            if (!infos.TryGetValue(t, out info))
                                            {
                                                info = CreateNewBindingInfo(t);
                                                infos[t] = info;
                                            }

                                            info.Constructors.Add(m.ConstructorInfo);
                                        }
                                        else
                                        {
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
                                }
                                break;
                        }
                    }
                }
            }
        }

        internal static CLRBindingGenerateInfo CreateNewBindingInfo(Type t)
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

        internal static List<string> GenerateDelegateBinding(List<Type> types, string outputPath)
        {
            if (types == null)
                types = new List<Type>(0);

            List<string> clsNames = new List<string>();

            foreach (var i in types)
            {
                var mi = i.GetMethod("Invoke");
                var miParameters = mi.GetParameters();
                if (mi.ReturnType == typeof(void) && miParameters.Length == 0)
                    continue;

                string clsName, realClsName, paramClsName, paramRealClsName;
                bool isByRef, paramIsByRef;
                i.GetClassName(out clsName, out realClsName, out isByRef);
                clsNames.Add(clsName);
                using (System.IO.StreamWriter sw = new System.IO.StreamWriter(outputPath + "/" + clsName + ".cs", false, new UTF8Encoding(false)))
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append(@"using System;
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
                    sb.AppendLine(clsName);
                    sb.AppendLine(@"    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {");
                    bool first = true;

                    if (mi.ReturnType != typeof(void))
                    {
                        sb.Append("            app.DelegateManager.RegisterFunctionDelegate<");
                        first = true;
                        foreach (var j in miParameters)
                        {
                            if (first)
                            {
                                first = false;
                            }
                            else
                                sb.Append(", ");
                            j.ParameterType.GetClassName(out paramClsName, out paramRealClsName, out paramIsByRef);
                            sb.Append(paramRealClsName);
                        }
                        if (!first)
                            sb.Append(", ");
                        mi.ReturnType.GetClassName(out paramClsName, out paramRealClsName, out paramIsByRef);
                        sb.Append(paramRealClsName);
                        sb.AppendLine("> ();");
                    }
                    else
                    {
                        sb.Append("            app.DelegateManager.RegisterMethodDelegate<");
                        first = true;
                        foreach (var j in miParameters)
                        {
                            if (first)
                            {
                                first = false;
                            }
                            else
                                sb.Append(", ");
                            j.ParameterType.GetClassName(out paramClsName, out paramRealClsName, out paramIsByRef);
                            sb.Append(paramRealClsName);
                        }
                        sb.AppendLine("> ();");
                    }
                    sb.AppendLine();

                    sb.Append("            app.DelegateManager.RegisterDelegateConvertor<");
                    sb.Append(realClsName);
                    sb.AppendLine(">((act) =>");
                    sb.AppendLine("            {");
                    sb.Append("                return new ");
                    sb.Append(realClsName);
                    sb.Append("((");
                    first = true;
                    foreach (var j in miParameters)
                    {
                        if (first)
                        {
                            first = false;
                        }
                        else
                            sb.Append(", ");
                        sb.Append(j.Name);
                    }
                    sb.AppendLine(") =>");
                    sb.AppendLine("                {");
                    if (mi.ReturnType != typeof(void))
                    {
                        sb.Append("                    return ((Func<");
                        first = true;
                        foreach (var j in miParameters)
                        {
                            if (first)
                            {
                                first = false;
                            }
                            else
                                sb.Append(", ");
                            j.ParameterType.GetClassName(out paramClsName, out paramRealClsName, out paramIsByRef);
                            sb.Append(paramRealClsName);
                        }
                        if (!first)
                            sb.Append(", ");
                        mi.ReturnType.GetClassName(out paramClsName, out paramRealClsName, out paramIsByRef);
                        sb.Append(paramRealClsName);
                    }
                    else
                    {
                        sb.Append("                    ((Action<");
                        first = true;
                        foreach (var j in miParameters)
                        {
                            if (first)
                            {
                                first = false;
                            }
                            else
                                sb.Append(", ");
                            j.ParameterType.GetClassName(out paramClsName, out paramRealClsName, out paramIsByRef);
                            sb.Append(paramRealClsName);
                        }
                    }
                    sb.Append(">)act)(");
                    first = true;
                    foreach (var j in miParameters)
                    {
                        if (first)
                        {
                            first = false;
                        }
                        else
                            sb.Append(", ");
                        sb.Append(j.Name);
                    }
                    sb.AppendLine(");");
                    sb.AppendLine("                });");
                    sb.AppendLine("            });");

                    sb.AppendLine("        }");
                    sb.AppendLine("    }");
                    sb.AppendLine("}");

                    sw.Write(Regex.Replace(sb.ToString(), "(?<!\r)\n", "\r\n"));
                    sw.Flush();
                }
            }

            return clsNames;
        }


        static private string SmartBindText = @"
//will auto register in unity
#if UNITY_5_3_OR_NEWER
        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.BeforeSceneLoad)]
#endif
        static private void RegisterBindingAction()
        {
            ILRuntime.Runtime.CLRBinding.CLRBindingUtils.RegisterBindingAction(Initialize);
        }
";

        internal static void GenerateBindingInitializeScript(List<string> clsNames, List<Type> valueTypeBinders, string outputPath)
        {
            if (!System.IO.Directory.Exists(outputPath))
                System.IO.Directory.CreateDirectory(outputPath);
            
            using (System.IO.StreamWriter sw = new System.IO.StreamWriter(outputPath + "/CLRBindings.cs", false, new UTF8Encoding(false)))
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(@"using System;
using System.Collections.Generic;
using System.Reflection;

namespace ILRuntime.Runtime.Generated
{
    class CLRBindings
    {
");
                sb.Append(SmartBindText);

                if (valueTypeBinders != null)
                {
                    sb.AppendLine();

                    foreach (var i in valueTypeBinders)
                    {
                        string clsName, realClsName;
                        bool isByRef;
                        i.GetClassName(out clsName, out realClsName, out isByRef);

                        sb.AppendLine(string.Format("        internal static ILRuntime.Runtime.Enviorment.ValueTypeBinder<{0}> s_{1}_Binder = null;", realClsName, clsName));
                    }

                    sb.AppendLine();
                }

                sb.AppendLine(@"        /// <summary>
        /// Initialize the CLR binding, please invoke this AFTER CLR Redirection registration
        /// </summary>
        public static void Initialize(ILRuntime.Runtime.Enviorment.AppDomain app)
        {");
                if (clsNames != null)
                {
                    foreach (var i in clsNames)
                    {
                        sb.Append("            ");
                        sb.Append(i);
                        sb.AppendLine(".Register(app);");
                    }
                }

                if (valueTypeBinders != null && valueTypeBinders.Count > 0)
                {
                    sb.AppendLine();

                    sb.AppendLine("            ILRuntime.CLR.TypeSystem.CLRType __clrType = null;");
                    foreach (var i in valueTypeBinders)
                    {
                        string clsName, realClsName;
                        bool isByRef;
                        i.GetClassName(out clsName, out realClsName, out isByRef);

                        sb.AppendLine(string.Format("            __clrType = (ILRuntime.CLR.TypeSystem.CLRType)app.GetType (typeof({0}));", realClsName));
                        sb.AppendLine(string.Format("            s_{0}_Binder = __clrType.ValueTypeBinder as ILRuntime.Runtime.Enviorment.ValueTypeBinder<{1}>;", clsName, realClsName));
                    }
                }
                sb.AppendLine(@"        }");

                sb.AppendLine(@"
        /// <summary>
        /// Release the CLR binding, please invoke this BEFORE ILRuntime Appdomain destroy
        /// </summary>
        public static void Shutdown(ILRuntime.Runtime.Enviorment.AppDomain app)
        {");
                if (valueTypeBinders != null)
                {
                    foreach (var i in valueTypeBinders)
                    {
                        string clsName, realClsName;
                        bool isByRef;
                        i.GetClassName(out clsName, out realClsName, out isByRef);

                        sb.AppendLine(string.Format("            s_{0}_Binder = null;", clsName));
                    }
                }
                sb.AppendLine(@"        }");

                sb.AppendLine(@"    }
}");
                sw.Write(Regex.Replace(sb.ToString(), "(?<!\r)\n", "\r\n"));
            }
        }

    }
}
