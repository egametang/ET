using dnlib.DotNet;
using HybridCLR.Editor.ABI;
using HybridCLR.Editor.Meta;
using HybridCLR.Editor.ReversePInvokeWrap;
using HybridCLR.Editor.Template;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using TypeInfo = HybridCLR.Editor.ABI.TypeInfo;
using CallingConvention = System.Runtime.InteropServices.CallingConvention;
using System.Security.Cryptography;
using TypeAttributes = dnlib.DotNet.TypeAttributes;

namespace HybridCLR.Editor.MethodBridge
{
    public class Generator
    {
        public class Options
        {
            public string TemplateCode { get; set; }

            public string OutputFile { get; set; }

            public IReadOnlyCollection<GenericMethod> GenericMethods { get; set; }

            public List<RawReversePInvokeMethodInfo> ReversePInvokeMethods { get; set; }

            public bool Development { get; set; }
        }

        private readonly List<GenericMethod> _genericMethods;

        private readonly List<RawReversePInvokeMethodInfo> _originalReversePInvokeMethods;

        private readonly string _templateCode;

        private readonly string _outputFile;

        private readonly bool _development;

        private readonly TypeCreator _typeCreator;

        private readonly HashSet<MethodDesc> _managed2nativeMethodSet = new HashSet<MethodDesc>();

        private readonly HashSet<MethodDesc> _native2managedMethodSet = new HashSet<MethodDesc>();

        private readonly HashSet<MethodDesc> _adjustThunkMethodSet = new HashSet<MethodDesc>();

        private List<ABIReversePInvokeMethodInfo> _reversePInvokeMethods;

        public Generator(Options options)
        {
            List<(GenericMethod, string)> genericMethodInfo = options.GenericMethods.Select(m => (m, m.ToString())).ToList();
            genericMethodInfo.Sort((a, b) => string.CompareOrdinal(a.Item2, b.Item2));
            _genericMethods = genericMethodInfo.Select(m => m.Item1).ToList();
            _originalReversePInvokeMethods = options.ReversePInvokeMethods;
            
            _templateCode = options.TemplateCode;
            _outputFile = options.OutputFile;
            _typeCreator = new TypeCreator();
            _development = options.Development;
        }

        private readonly Dictionary<string, TypeInfo> _sig2Types = new Dictionary<string, TypeInfo>();

        private TypeInfo GetSharedTypeInfo(TypeSig type)
        {
            var typeInfo = _typeCreator.CreateTypeInfo(type);
            if (!typeInfo.IsStruct)
            {
                return typeInfo;
            }
            string sigName = ToFullName(typeInfo.Klass);
            if (!_sig2Types.TryGetValue(sigName, out var sharedTypeInfo))
            {
                sharedTypeInfo = typeInfo;
                _sig2Types.Add(sigName, sharedTypeInfo);
            }
            return sharedTypeInfo;
        }

        private MethodDesc CreateMethodDesc(MethodDef methodDef, bool forceRemoveThis, TypeSig returnType, List<TypeSig> parameters)
        {
            var paramInfos = new List<ParamInfo>();
            if (forceRemoveThis && !methodDef.IsStatic)
            {
                parameters.RemoveAt(0);
            }
            if (returnType.ContainsGenericParameter)
            {
                throw new Exception($"[PreservedMethod] method:{methodDef} has generic parameters");
            }
            foreach (var paramInfo in parameters)
            {
                if (paramInfo.ContainsGenericParameter)
                {
                    throw new Exception($"[PreservedMethod] method:{methodDef} has generic parameters");
                }
                paramInfos.Add(new ParamInfo() { Type = GetSharedTypeInfo(paramInfo) });
            }
            var mbs = new MethodDesc()
            {
                MethodDef = methodDef,
                ReturnInfo = new ReturnInfo() { Type = returnType != null ? GetSharedTypeInfo(returnType) : TypeInfo.s_void },
                ParamInfos = paramInfos,
            };
            return mbs;
        }

        private void AddManaged2NativeMethod(MethodDesc method)
        {
            method.Init();
            _managed2nativeMethodSet.Add(method);
        }

        private void AddNative2ManagedMethod(MethodDesc method)
        {
            method.Init();
            _native2managedMethodSet.Add(method);
        }

        private void AddAdjustThunkMethod(MethodDesc method)
        {
            method.Init();
            _adjustThunkMethodSet.Add(method);
        }

        private void ProcessMethod(MethodDef method, List<TypeSig> klassInst, List<TypeSig> methodInst)
        {
            if (method.IsPrivate || (method.IsAssembly && !method.IsPublic && !method.IsFamily))
            {
                if (klassInst == null && methodInst == null)
                {
                    return;
                }
                else
                {
                    //Debug.Log($"[PreservedMethod] method:{method}");
                }
            }
            ICorLibTypes corLibTypes = method.Module.CorLibTypes;
            TypeSig returnType;
            List<TypeSig> parameters;
            if (klassInst == null && methodInst == null)
            {
                if (method.HasGenericParameters)
                {
                    throw new Exception($"[PreservedMethod] method:{method} has generic parameters");
                }
                returnType = MetaUtil.ToShareTypeSig(corLibTypes, method.ReturnType);
                parameters = method.Parameters.Select(p => MetaUtil.ToShareTypeSig(corLibTypes, p.Type)).ToList();
            }
            else
            {
                var gc = new GenericArgumentContext(klassInst, methodInst);
                returnType = MetaUtil.ToShareTypeSig(corLibTypes, MetaUtil.Inflate(method.ReturnType, gc));
                parameters = method.Parameters.Select(p => MetaUtil.ToShareTypeSig(corLibTypes, MetaUtil.Inflate(p.Type, gc))).ToList();
            }

            var m2nMethod = CreateMethodDesc(method, false, returnType, parameters);
            AddManaged2NativeMethod(m2nMethod);

            if (method.IsVirtual)
            {
                if (method.DeclaringType.IsInterface)
                {
                    AddAdjustThunkMethod(m2nMethod);
                }
                //var adjustThunkMethod = CreateMethodDesc(method, true, returnType, parameters);
                AddNative2ManagedMethod(m2nMethod);
            }
            if (method.Name == "Invoke" && method.DeclaringType.IsDelegate)
            {
                var openMethod = CreateMethodDesc(method, true, returnType, parameters);
                AddNative2ManagedMethod(openMethod);
            }
        }

        private void PrepareMethodBridges()
        {
            foreach (var method in _genericMethods)
            {
                ProcessMethod(method.Method, method.KlassInst, method.MethodInst);
            }
            foreach (var reversePInvokeMethod in _originalReversePInvokeMethods)
            {
                MethodDef method = reversePInvokeMethod.Method;
                ICorLibTypes corLibTypes = method.Module.CorLibTypes;

                var returnType = MetaUtil.ToShareTypeSig(corLibTypes, method.ReturnType);
                var parameters = method.Parameters.Select(p => MetaUtil.ToShareTypeSig(corLibTypes, p.Type)).ToList();
                var sharedMethod = CreateMethodDesc(method, true, returnType, parameters);
                sharedMethod.Init();
                AddNative2ManagedMethod(sharedMethod);
            }
        }

        static void CheckUnique(IEnumerable<string> names)
        {
            var set = new HashSet<string>();
            foreach (var name in names)
            {
                if (!set.Add(name))
                {
                    throw new Exception($"[CheckUnique] duplicate name:{name}");
                }
            }
        }


        private List<MethodDesc> _managed2NativeMethodList0;
        private List<MethodDesc> _native2ManagedMethodList0;
        private List<MethodDesc> _adjustThunkMethodList0;

        private List<TypeInfo> _structTypes0;

        private void CollectTypesAndMethods()
        {
            _managed2NativeMethodList0 = _managed2nativeMethodSet.ToList();
            _managed2NativeMethodList0.Sort((a, b) => string.CompareOrdinal(a.Sig, b.Sig));

            _native2ManagedMethodList0 = _native2managedMethodSet.ToList();
            _native2ManagedMethodList0.Sort((a, b) => string.CompareOrdinal(a.Sig, b.Sig));

            _adjustThunkMethodList0 = _adjustThunkMethodSet.ToList();
            _adjustThunkMethodList0.Sort((a, b) => string.CompareOrdinal(a.Sig, b.Sig));


            var structTypeSet = new HashSet<TypeInfo>();
            CollectStructDefs(_managed2NativeMethodList0, structTypeSet);
            CollectStructDefs(_native2ManagedMethodList0, structTypeSet);
            CollectStructDefs(_adjustThunkMethodList0, structTypeSet);
            _structTypes0 = structTypeSet.ToList();
            _structTypes0.Sort((a, b) => a.TypeId - b.TypeId);

            CheckUnique(_structTypes0.Select(t => ToFullName(t.Klass)));
            CheckUnique(_structTypes0.Select(t => t.CreateSigName()));

            Debug.LogFormat("== before optimization struct:{3} managed2native:{0} native2managed:{1} adjustThunk:{2}",
                _managed2NativeMethodList0.Count, _native2ManagedMethodList0.Count, _adjustThunkMethodList0.Count, _structTypes0.Count);
        }

        private class AnalyzeFieldInfo
        {
            public FieldDef field;

            public TypeInfo type;
        }

        private class AnalyzeTypeInfo
        {
            public TypeInfo toSharedType;
            public List<AnalyzeFieldInfo> fields;
            public string signature;
            public ClassLayout classLayout;
            public TypeAttributes layout;
        }

        private readonly Dictionary<TypeInfo, AnalyzeTypeInfo> _analyzeTypeInfos = new Dictionary<TypeInfo, AnalyzeTypeInfo>();

        private readonly Dictionary<string, TypeInfo> _signature2Type = new Dictionary<string, TypeInfo>();

        private AnalyzeTypeInfo CalculateAnalyzeTypeInfoBasic(TypeInfo typeInfo)
        {
            TypeSig type = typeInfo.Klass;
            TypeDef typeDef = type.ToTypeDefOrRef().ResolveTypeDefThrow();

            List<TypeSig> klassInst = type.ToGenericInstSig()?.GenericArguments?.ToList();
            GenericArgumentContext ctx = klassInst != null ? new GenericArgumentContext(klassInst, null) : null;

            ClassLayout sa = typeDef.ClassLayout;
            var analyzeTypeInfo = new AnalyzeTypeInfo()
            {
                classLayout = sa,
                layout = typeDef.Layout,
            };

            // don't share type with explicit layout
            if (sa != null)
            {
                analyzeTypeInfo.toSharedType = typeInfo;
                analyzeTypeInfo.signature = typeInfo.CreateSigName();
                _signature2Type.Add(analyzeTypeInfo.signature, typeInfo);
                return analyzeTypeInfo;
            }

            var fields = analyzeTypeInfo.fields = new List<AnalyzeFieldInfo>();

            foreach (FieldDef field in typeDef.Fields)
            {
                if (field.IsStatic)
                {
                    continue;
                }
                TypeSig fieldType = ctx != null ? MetaUtil.Inflate(field.FieldType, ctx) : field.FieldType;
                fields.Add(new AnalyzeFieldInfo { field = field, type = GetSharedTypeInfo(fieldType) });
            }
            return analyzeTypeInfo;
        }

        private string GetOrCalculateTypeInfoSignature(TypeInfo typeInfo)
        {
            if (!typeInfo.IsStruct)
            {
                return typeInfo.CreateSigName();
            }

            var ati = _analyzeTypeInfos[typeInfo];

            //if (_analyzeTypeInfos.TryGetValue(typeInfo, out var ati))
            //{
            //    return ati.signature;
            //}
            //ati = CalculateAnalyzeTypeInfoBasic(typeInfo);
            //_analyzeTypeInfos.Add(typeInfo, ati);
            if (ati.signature != null)
            {
                return ati.signature;
            }
            
            var sigBuf = new StringBuilder();
            if (ati.classLayout != null)
            {
                sigBuf.Append($"[{ati.classLayout.ClassSize}|{ati.classLayout.PackingSize}|{ati.classLayout}]");
            }
            if (ati.layout != 0)
            {
                sigBuf.Append($"[{(int)ati.layout}]");
            }
            
            foreach (var field in ati.fields)
            {
                string fieldOffset = field.field.FieldOffset != null ? field.field.FieldOffset.ToString() + "|" : "";
                sigBuf.Append("{" + fieldOffset + GetOrCalculateTypeInfoSignature(ToIsomorphicType(field.type)) + "}");
            }
            return ati.signature = sigBuf.ToString();
        }

        private TypeInfo ToIsomorphicType(TypeInfo type)
        {
            if (!type.IsStruct)
            {
                return type;
            }
            if (!_analyzeTypeInfos.TryGetValue(type, out var ati))
            {
                ati = CalculateAnalyzeTypeInfoBasic(type);
                _analyzeTypeInfos.Add(type, ati);
            }
            if (ati.toSharedType == null)
            {
                string signature = GetOrCalculateTypeInfoSignature(type);
                Debug.Assert(signature == ati.signature);
                if (_signature2Type.TryGetValue(signature, out var sharedType))
                {
                    // Debug.Log($"[ToIsomorphicType] type:{type.Klass} ==> sharedType:{sharedType.Klass} signature:{signature} ");
                    ati.toSharedType = sharedType;
                }
                else
                {
                    ati.toSharedType = type;
                    _signature2Type.Add(signature, type);
                }
            }
            return ati.toSharedType;
        }

        private MethodDesc ToIsomorphicMethod(MethodDesc method)
        {
            var paramInfos = new List<ParamInfo>();
            foreach (var paramInfo in method.ParamInfos)
            {
                paramInfos.Add(new ParamInfo() { Type = ToIsomorphicType(paramInfo.Type) });
            }
            var mbs = new MethodDesc()
            {
                MethodDef = method.MethodDef,
                ReturnInfo = new ReturnInfo() { Type = ToIsomorphicType(method.ReturnInfo.Type) },
                ParamInfos = paramInfos,
            };
            mbs.Init();
            return mbs;
        }

        private List<MethodDesc> _managed2NativeMethodList;
        private List<MethodDesc> _native2ManagedMethodList;
        private List<MethodDesc> _adjustThunkMethodList;

        private List<TypeInfo> structTypes;

        private void BuildAnalyzeTypeInfos()
        {
            foreach (var type in _structTypes0)
            {
                ToIsomorphicType(type);
            }
            structTypes = _signature2Type.Values.ToList();
            structTypes.Sort((a, b) => a.TypeId - b.TypeId);
        }

        private List<MethodDesc> ToUniqueOrderedList(List<MethodDesc> methods)
        {
            var methodMap = new SortedDictionary<string, MethodDesc>();
            foreach (var method in methods)
            {
                var sharedMethod = ToIsomorphicMethod(method);
                var sig = sharedMethod.Sig;
                if (!methodMap.TryGetValue(sig, out var _))
                {
                    methodMap.Add(sig, sharedMethod);
                }
            }
            return methodMap.Values.ToList();
        }



        private static string MakeSignature(MethodDesc desc, CallingConvention CallingConventionention)
        {
            string convStr = ((char)('A' + (int)CallingConventionention - 1)).ToString();
            return $"{convStr}{desc.Sig}";
        }

        private static CallingConvention GetCallingConvention(MethodDef method)
        {
            var monoPInvokeCallbackAttr = method.CustomAttributes.FirstOrDefault(ca => ca.AttributeType.Name == "MonoPInvokeCallbackAttribute");
            if (monoPInvokeCallbackAttr == null)
            {
                return CallingConvention.Winapi;
            }
            object delegateTypeSig = monoPInvokeCallbackAttr.ConstructorArguments[0].Value;

            TypeDef delegateTypeDef;
            if (delegateTypeSig is ClassSig classSig)
            {
                delegateTypeDef = classSig.TypeDefOrRef.ResolveTypeDefThrow();
            }
            else if (delegateTypeSig is GenericInstSig genericInstSig)
            {
                delegateTypeDef = genericInstSig.GenericType.TypeDefOrRef.ResolveTypeDefThrow();
            }
            else
            {
                delegateTypeDef = null;
            }

            if (delegateTypeDef == null)
            {
                throw new NotSupportedException($"Unsupported delegate type: {delegateTypeSig}");
            }
            var attr = delegateTypeDef.CustomAttributes.FirstOrDefault(ca => ca.AttributeType.FullName == "System.Runtime.InteropServices.UnmanagedFunctionPointerAttribute");
            if (attr == null)
            {
                return CallingConvention.Winapi;
            }
            var conv = attr.ConstructorArguments[0].Value;
            return (CallingConvention)conv;
        }

        private List<ABIReversePInvokeMethodInfo> BuildABIMethods(List<RawReversePInvokeMethodInfo> rawMethods)
        {
            var methodsBySig = new Dictionary<string, ABIReversePInvokeMethodInfo>();
            foreach (var method in rawMethods)
            {
                var sharedMethod = new MethodDesc
                {
                    MethodDef = method.Method,
                    ReturnInfo = new ReturnInfo { Type = _typeCreator.CreateTypeInfo(method.Method.ReturnType) },
                    ParamInfos = method.Method.Parameters.Select(p => new ParamInfo { Type = _typeCreator.CreateTypeInfo(p.Type) }).ToList(),
                };
                sharedMethod.Init();
                sharedMethod = ToIsomorphicMethod(sharedMethod);

                CallingConvention callingConv = GetCallingConvention(method.Method);
                string signature = MakeSignature(sharedMethod, callingConv);

                if (!methodsBySig.TryGetValue(signature, out var arm))
                {
                    arm = new ABIReversePInvokeMethodInfo()
                    {
                        Method = sharedMethod,
                        Signature = signature,
                        Count = 0,
                        Callvention = callingConv,
                    };
                    methodsBySig.Add(signature, arm);
                }
                int preserveCount = method.GenerationAttribute != null ? (int)method.GenerationAttribute.ConstructorArguments[0].Value : 1;
                arm.Count += preserveCount;
            }
            var newMethods = methodsBySig.Values.ToList();
            newMethods.Sort((a, b) => string.CompareOrdinal(a.Signature, b.Signature));
            return newMethods;
        }

        private void BuildOptimizedMethods()
        {
            _managed2NativeMethodList = ToUniqueOrderedList(_managed2NativeMethodList0);
            _native2ManagedMethodList = ToUniqueOrderedList(_native2ManagedMethodList0);
            _adjustThunkMethodList = ToUniqueOrderedList(_adjustThunkMethodList0);
            _reversePInvokeMethods = BuildABIMethods(_originalReversePInvokeMethods);
        }

        private void OptimizationTypesAndMethods()
        {
            BuildAnalyzeTypeInfos();
            BuildOptimizedMethods();
            Debug.LogFormat("== after optimization struct:{3} managed2native:{0} native2managed:{1} adjustThunk:{2}",
                               _managed2NativeMethodList.Count, _native2ManagedMethodList.Count, _adjustThunkMethodList.Count, structTypes.Count);
        }

        private void GenerateCode()
        {
            var frr = new FileRegionReplace(_templateCode);

            List<string> lines = new List<string>(20_0000)
            {
                "\n",
                $"// DEVELOPMENT={(_development ? 1 : 0)}",
                "\n"
            };

            var classInfos = new List<ClassInfo>();
            var classTypeSet = new HashSet<TypeInfo>();
            foreach (var type in structTypes)
            {
                GenerateClassInfo(type, classTypeSet, classInfos);
            }

            GenerateStructDefines(classInfos, lines);

            // use structTypes0 to generate signature
            GenerateStructureSignatureStub(_structTypes0, lines);

            foreach (var method in _managed2NativeMethodList)
            {
                GenerateManaged2NativeMethod(method, lines);
            }

            GenerateManaged2NativeStub(_managed2NativeMethodList, lines);

            foreach (var method in _native2ManagedMethodList)
            {
                GenerateNative2ManagedMethod(method, lines);
            }

            GenerateNative2ManagedStub(_native2ManagedMethodList, lines);

            foreach (var method in _adjustThunkMethodList)
            {
                GenerateAdjustThunkMethod(method, lines);
            }

            GenerateAdjustThunkStub(_adjustThunkMethodList, lines);

            GenerateReversePInvokeWrappers(_reversePInvokeMethods, lines);

            frr.Replace("CODE", string.Join("\n", lines));

            Directory.CreateDirectory(Path.GetDirectoryName(_outputFile));

            frr.Commit(_outputFile);
        }

        private static string GetIl2cppCallConventionName(CallingConvention conv)
        {
            switch (conv)
            {
                case 0:
                case CallingConvention.Winapi:
                    return "DEFAULT_CALL";
                case CallingConvention.Cdecl:
                    return "CDECL";
                case CallingConvention.StdCall:
                    return "STDCALL";
                case CallingConvention.ThisCall:
                    return "THISCALL";
                case CallingConvention.FastCall:
                    return "FASTCALL";
                default:
                    throw new NotSupportedException($"Unsupported CallingConvention {conv}");
            }
        }

        private void GenerateReversePInvokeWrappers(List<ABIReversePInvokeMethodInfo> methods, List<string> lines)
        {
            int methodIndex = 0;
            var stubCodes = new List<string>();
            foreach (var methodInfo in methods)
            {
                MethodDesc method = methodInfo.Method;
                string il2cppCallConventionName = GetIl2cppCallConventionName(methodInfo.Callvention);
                string paramDeclaringListWithoutMethodInfoStr = string.Join(", ", method.ParamInfos.Select(p => $"{p.Type.GetTypeName()} __arg{p.Index}"));
                string paramNameListWithoutMethodInfoStr = string.Join(", ", method.ParamInfos.Select(p => $"__arg{p.Index}").Concat(new string[] { "method" }));
                string paramTypeListWithMethodInfoStr = string.Join(", ", method.ParamInfos.Select(p => $"{p.Type.GetTypeName()}").Concat(new string[] { "const MethodInfo*" }));
                string methodTypeDef = $"typedef {method.ReturnInfo.Type.GetTypeName()} (*Callback)({paramTypeListWithMethodInfoStr})";
                for (int i = 0; i < methodInfo.Count; i++, methodIndex++)
                {
                    lines.Add($@"
{method.ReturnInfo.Type.GetTypeName()} {il2cppCallConventionName} __ReversePInvokeMethod_{methodIndex}({paramDeclaringListWithoutMethodInfoStr})
{{
    il2cpp::vm::ScopedThreadAttacher _vmThreadHelper;
    const MethodInfo* method = InterpreterModule::GetMethodInfoByReversePInvokeWrapperIndex({methodIndex});
    {methodTypeDef};
    {(method.ReturnInfo.IsVoid ? "" : "return ")}((Callback)(method->methodPointerCallByInterp))({paramNameListWithoutMethodInfoStr});
}}
        ");
                    stubCodes.Add($"\t{{\"{methodInfo.Signature}\", (Il2CppMethodPointer)__ReversePInvokeMethod_{methodIndex}}},");
                }
                Debug.Log($"[ReversePInvokeWrap.Generator] method:{method.MethodDef} wrapperCount:{methodInfo.Count}");
            }

            lines.Add(@"
const ReversePInvokeMethodData hybridclr::interpreter::g_reversePInvokeMethodStub[]
{
");
            lines.AddRange(stubCodes);

            lines.Add(@"
    {nullptr, nullptr},
};
");
        }

        public void Generate()
        {
            PrepareMethodBridges();
            CollectTypesAndMethods();
            OptimizationTypesAndMethods();
            GenerateCode();
        }

        private void CollectStructDefs(List<MethodDesc> methods, HashSet<TypeInfo> structTypes)
        {
            foreach (var method in methods)
            {
                foreach(var paramInfo in method.ParamInfos)
                {
                    if (paramInfo.Type.IsStruct)
                    {
                        structTypes.Add(paramInfo.Type);
                        if (paramInfo.Type.Klass.ContainsGenericParameter)
                        {
                            throw new Exception($"[CollectStructDefs] method:{method.MethodDef} type:{paramInfo.Type.Klass} contains generic parameter");
                        }
                    }
                    
                }
                if (method.ReturnInfo.Type.IsStruct)
                {
                    structTypes.Add(method.ReturnInfo.Type);
                    if (method.ReturnInfo.Type.Klass.ContainsGenericParameter)
                    {
                        throw new Exception($"[CollectStructDefs] method:{method.MethodDef} type:{method.ReturnInfo.Type.Klass} contains generic parameter");
                    }
                }
            }
            
        }

        class FieldInfo
        {
            public FieldDef field;
            public TypeInfo type;
        }

        class ClassInfo
        {
            public TypeInfo type;

            public TypeDef typeDef;

            public List<FieldInfo> fields = new List<FieldInfo>();

            public ClassLayout layout;
        }

        private void GenerateClassInfo(TypeInfo type, HashSet<TypeInfo> typeSet, List<ClassInfo> classInfos)
        {
            if (!typeSet.Add(type))
            {
                return;
            }
            TypeSig typeSig = type.Klass;
            var fields = new List<FieldInfo>();

            TypeDef typeDef = typeSig.ToTypeDefOrRef().ResolveTypeDefThrow();

            List<TypeSig> klassInst = typeSig.ToGenericInstSig()?.GenericArguments?.ToList();
            GenericArgumentContext ctx = klassInst != null ? new GenericArgumentContext(klassInst, null) : null;

            ClassLayout sa = typeDef.ClassLayout;
           
            ICorLibTypes corLibTypes = typeDef.Module.CorLibTypes;
            foreach (FieldDef field in typeDef.Fields)
            {
                if (field.IsStatic)
                {
                    continue;
                }
                TypeSig fieldType = ctx != null ? MetaUtil.Inflate(field.FieldType, ctx) : field.FieldType;
                fieldType = MetaUtil.ToShareTypeSig(corLibTypes, fieldType);
                var fieldTypeInfo = ToIsomorphicType(_typeCreator.CreateTypeInfo(fieldType));
                if (fieldTypeInfo.IsStruct)
                {
                    GenerateClassInfo(fieldTypeInfo, typeSet, classInfos);
                }
                fields.Add(new FieldInfo { field = field, type = fieldTypeInfo });
            }
            classInfos.Add(new ClassInfo() { type = type, typeDef = typeDef, fields = fields, layout = sa });
        }

        private void GenerateStructDefines(List<ClassInfo> classInfos, List<string> lines)
        {
            foreach (var ci in classInfos)
            {
                lines.Add($"// {ci.type.Klass}");
                uint packingSize = ci.layout?.PackingSize ?? 0;
                if (packingSize != 0)
                {
                    lines.Add($"#pragma pack(push, {packingSize})");
                }
                uint classSize = ci.layout?.ClassSize ?? 0;
               
                if (ci.typeDef.IsExplicitLayout)
                {
                    lines.Add($"union {ci.type.GetTypeName()} {{");
                    if (classSize > 0)
                    {
                        lines.Add($"\tstruct {{ char __fieldSize_offsetPadding[{classSize}];}};");
                    }
                    int index = 0;
                    foreach (var field in ci.fields)
                    {
                        uint offset = field.field.FieldOffset.Value;
                        string fieldName = $"__{index}";
                        string commentFieldName = $"{field.field.Name}";
                        lines.Add("\t#pragma pack(push, 1)");
                        lines.Add($"\tstruct {{ {(offset > 0 ? $"char {fieldName}_offsetPadding[{offset}];" : "")}  {field.type.GetTypeName()} {fieldName};}}; // {commentFieldName}");
                        lines.Add($"\t#pragma pack(pop)");
                        lines.Add($"\tstruct {{ {field.type.GetTypeName()} {fieldName}_forAlignmentOnly;}}; // {commentFieldName}");
                        ++index;
                    }
                }
                else
                {
                    lines.Add($"{(classSize > 0 ? "union" : "struct")} {ci.type.GetTypeName()} {{");
                    if (classSize > 0)
                    {
                        lines.Add($"\tstruct {{ char __fieldSize_offsetPadding[{classSize}];}};");
                        lines.Add("\tstruct {");
                    }
                    int index = 0;
                    foreach (var field in ci.fields)
                    {
                        string fieldName = $"__{index}";
                        string commentFieldName = $"{field.field.Name}";
                        lines.Add($"\t{field.type.GetTypeName()} {fieldName}; // {commentFieldName}");
                        ++index;
                    }
                    if (classSize > 0)
                    {
                        lines.Add("\t};");
                    }
                }
                lines.Add("};");
                if (packingSize != 0)
                {
                    lines.Add($"#pragma pack(pop)");
                }
            }
        }

        public const string SigOfObj = "u";

        public static string ToFullName(TypeSig type)
        {
            type = type.RemovePinnedAndModifiers();
            switch (type.ElementType)
            {
                case ElementType.Void: return "v";
                case ElementType.Boolean: return "u1";
                case ElementType.I1: return "i1";
                case ElementType.U1: return "u1";
                case ElementType.I2: return "i2";
                case ElementType.Char:
                case ElementType.U2: return "u2";
                case ElementType.I4: return "i4";
                case ElementType.U4: return "u4";
                case ElementType.I8: return "i8";
                case ElementType.U8: return "u8";
                case ElementType.R4: return "r4";
                case ElementType.R8: return "r8";
                case ElementType.I: return "i";
                case ElementType.U:
                case ElementType.String:
                case ElementType.Ptr:
                case ElementType.ByRef:
                case ElementType.Class:
                case ElementType.Array:
                case ElementType.SZArray:
                case ElementType.FnPtr:
                case ElementType.Object:
                    return SigOfObj;
                case ElementType.Module:
                case ElementType.Var:
                case ElementType.MVar:
                    throw new NotSupportedException($"ToFullName type:{type}");
                case ElementType.TypedByRef: return TypeInfo.strTypedByRef;
                case ElementType.ValueType:
                {
                    TypeDef typeDef = type.ToTypeDefOrRef().ResolveTypeDef();
                    if (typeDef == null)
                    {
                        throw new Exception($"type:{type} definition could not be found. Please try `HybridCLR/Genergate/LinkXml`, then Build once to generate the AOT dll, and then regenerate the bridge function");
                    }
                    if (typeDef.IsEnum)
                    {
                        return ToFullName(typeDef.GetEnumUnderlyingType());
                    }
                    return ToValueTypeFullName((ClassOrValueTypeSig)type);
                }
                case ElementType.GenericInst:
                    {
                        GenericInstSig gis = (GenericInstSig)type;
                        if (!gis.GenericType.IsValueType)
                        {
                            return SigOfObj;
                        }
                        TypeDef typeDef = gis.GenericType.ToTypeDefOrRef().ResolveTypeDef();
                        if (typeDef.IsEnum)
                        {
                            return ToFullName(typeDef.GetEnumUnderlyingType());
                        }
                        return $"{ToValueTypeFullName(gis.GenericType)}<{string.Join(",", gis.GenericArguments.Select(a => ToFullName(a)))}>";
                    }
                default: throw new NotSupportedException($"{type.ElementType}");
            }
        }

        private static bool IsSystemOrUnityAssembly(ModuleDef module)
        {
            if (module.IsCoreLibraryModule == true)
            {
                return true;
            }
            string assName = module.Assembly.Name.String;
            return assName.StartsWith("System.") || assName.StartsWith("UnityEngine.");
        }

        private static string ToValueTypeFullName(ClassOrValueTypeSig type)
        {
            TypeDef typeDef = type.ToTypeDefOrRef().ResolveTypeDef();
            if (typeDef == null)
            {
                throw new Exception($"type:{type} resolve fail");
            }

            if (typeDef.DeclaringType != null)
            {
                return $"{ToValueTypeFullName((ClassOrValueTypeSig)typeDef.DeclaringType.ToTypeSig())}/{typeDef.Name}";
            }

            if (IsSystemOrUnityAssembly(typeDef.Module))
            {
                return type.FullName;
            }
            return $"{Path.GetFileNameWithoutExtension(typeDef.Module.Name)}:{typeDef.FullName}";
        }

        public void GenerateStructureSignatureStub(List<TypeInfo> types, List<string> lines)
        {
            lines.Add("const FullName2Signature hybridclr::interpreter::g_fullName2SignatureStub[] = {");
            foreach (var type in types)
            {
                TypeInfo isoType = ToIsomorphicType(type);
                lines.Add($"\t{{\"{ToFullName(type.Klass)}\", \"{isoType.CreateSigName()}\"}},");
            }
            lines.Add("\t{ nullptr, nullptr},");
            lines.Add("};");
        }

        public void GenerateManaged2NativeStub(List<MethodDesc> methods, List<string> lines)
        {
            lines.Add($@"
const Managed2NativeMethodInfo hybridclr::interpreter::g_managed2nativeStub[] = 
{{
");

            foreach (var method in methods)
            {
                lines.Add($"\t{{\"{method.CreateInvokeSigName()}\", __M2N_{method.CreateInvokeSigName()}}},");
            }

            lines.Add($"\t{{nullptr, nullptr}},");
            lines.Add("};");
        }

        public void GenerateNative2ManagedStub(List<MethodDesc> methods, List<string> lines)
        {
            lines.Add($@"
const Native2ManagedMethodInfo hybridclr::interpreter::g_native2managedStub[] = 
{{
");

            foreach (var method in methods)
            {
                lines.Add($"\t{{\"{method.CreateInvokeSigName()}\", (Il2CppMethodPointer)__N2M_{method.CreateInvokeSigName()}}},");
            }

            lines.Add($"\t{{nullptr, nullptr}},");
            lines.Add("};");
        }

        public void GenerateAdjustThunkStub(List<MethodDesc> methods, List<string> lines)
        {
            lines.Add($@"
const NativeAdjustThunkMethodInfo hybridclr::interpreter::g_adjustThunkStub[] = 
{{
");

            foreach (var method in methods)
            {
                lines.Add($"\t{{\"{method.CreateInvokeSigName()}\", (Il2CppMethodPointer)__N2M_AdjustorThunk_{method.CreateCallSigName()}}},");
            }

            lines.Add($"\t{{nullptr, nullptr}},");
            lines.Add("};");
        }

        private string GetManaged2NativePassParam(TypeInfo type, string varName)
        {
            return $"M2NFromValueOrAddress<{type.GetTypeName()}>({varName})";
        }

        private string GetNative2ManagedPassParam(TypeInfo type, string varName)
        {
            return type.NeedExpandValue() ? $"(uint64_t)({varName})" : $"N2MAsUint64ValueOrAddress<{type.GetTypeName()}>({varName})";
        }

        public void GenerateManaged2NativeMethod(MethodDesc method, List<string> lines)
        {
            string paramListStr = string.Join(", ", method.ParamInfos.Select(p => $"{p.Type.GetTypeName()} __arg{p.Index}").Concat(new string[] { "const MethodInfo* method" }));
            string paramNameListStr = string.Join(", ", method.ParamInfos.Select(p => GetManaged2NativePassParam(p.Type, $"localVarBase+argVarIndexs[{p.Index}]")).Concat(new string[] { "method" }));

            lines.Add($@"
static void __M2N_{method.CreateCallSigName()}(const MethodInfo* method, uint16_t* argVarIndexs, StackObject* localVarBase, void* ret)
{{
    typedef {method.ReturnInfo.Type.GetTypeName()} (*NativeMethod)({paramListStr});
    {(!method.ReturnInfo.IsVoid ? $"*({method.ReturnInfo.Type.GetTypeName()}*)ret = " : "")}((NativeMethod)(method->methodPointerCallByInterp))({paramNameListStr});
}}
");
        }

        public string GenerateArgumentSizeAndOffset(List<ParamInfo> paramInfos)
        {
            StringBuilder s = new StringBuilder();
            int index = 0;
            foreach (var param in paramInfos)
            {
                s.AppendLine($"\tconstexpr int __ARG_OFFSET_{index}__ = {(index > 0 ? $"__ARG_OFFSET_{index - 1}__ + __ARG_SIZE_{index-1}__" : "0")};");
                s.AppendLine($"\tconstexpr int __ARG_SIZE_{index}__ = (sizeof(__arg{index}) + 7)/8;");
                index++;
            }
            s.AppendLine($"\tconstexpr int __TOTAL_ARG_SIZE__ = {(paramInfos.Count > 0 ? $"__ARG_OFFSET_{index-1}__ + __ARG_SIZE_{index-1}__" : "1")};");
            return s.ToString();
        }

        public string GenerateCopyArgumentToInterpreterStack(List<ParamInfo> paramInfos)            
        {
            StringBuilder s = new StringBuilder();
            int index = 0;
            foreach (var param in paramInfos)
            {
                if (param.Type.IsPrimitiveType)
                {
                    if (param.Type.NeedExpandValue())
                    {
                        s.AppendLine($"\targs[__ARG_OFFSET_{index}__].u64 = __arg{index};");
                    }
                    else
                    {
                        s.AppendLine($"\t*({param.Type.GetTypeName()}*)(args + __ARG_OFFSET_{index}__) = __arg{index};");
                    }
                }
                else
                {
                    s.AppendLine($"\t*({param.Type.GetTypeName()}*)(args + __ARG_OFFSET_{index}__) = __arg{index};");
                }
                index++;
            }
            return s.ToString();
        }

        private void GenerateNative2ManagedMethod0(MethodDesc method, bool adjustorThunk, List<string> lines)
        {
            string paramListStr = string.Join(", ", method.ParamInfos.Select(p => $"{p.Type.GetTypeName()} __arg{p.Index}").Concat(new string[] { "const MethodInfo* method" }));
            lines.Add($@"
static {method.ReturnInfo.Type.GetTypeName()} __N2M_{(adjustorThunk ? "AdjustorThunk_" : "")}{method.CreateCallSigName()}({paramListStr})
{{
    {(adjustorThunk ? "__arg0 += sizeof(Il2CppObject);" : "")}
{GenerateArgumentSizeAndOffset(method.ParamInfos)}
    StackObject args[__TOTAL_ARG_SIZE__];
{GenerateCopyArgumentToInterpreterStack(method.ParamInfos)}
    {(method.ReturnInfo.IsVoid ? "Interpreter::Execute(method, args, nullptr);" : $"{method.ReturnInfo.Type.GetTypeName()} ret; Interpreter::Execute(method, args, &ret); return ret;")}
}}
");
        }

        public void GenerateNative2ManagedMethod(MethodDesc method, List<string> lines)
        {
            GenerateNative2ManagedMethod0(method, false, lines);
        }

        public void GenerateAdjustThunkMethod(MethodDesc method, List<string> lines)
        {
            GenerateNative2ManagedMethod0(method, true, lines);
        }
    }
}
