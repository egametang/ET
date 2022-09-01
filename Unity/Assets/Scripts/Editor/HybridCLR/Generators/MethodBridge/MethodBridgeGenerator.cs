using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace HybridCLR.Editor.Generators.MethodBridge
{

    public class TypeGenInfo
    {
        public Type Type { get; set; }

        public List<MethodInfo> GenericMethods { get; set; }
    }

    public class MethodBridgeGeneratorOptions
    {
        public List<Assembly> HotfixAssemblies { get; set; }

        public List<Assembly> AllAssemblies { get; set; }

        public PlatformABI CallConvention { get; set; }

        public string OutputFile { get; set; }

        public bool Optimized { get; set; }
    }

    public class MethodBridgeGenerator
    {
        private readonly HashSet<Assembly> _hotfixAssemblies;

        private readonly List<Assembly> _assemblies;

        private readonly PlatformABI _callConvention;

        private readonly string _outputFile;

        public readonly bool _optimized;

        private readonly IPlatformAdaptor _platformAdaptor;

        private readonly HashSet<MethodBridgeSig> _managed2nativeMethodSet = new HashSet<MethodBridgeSig>();

        private List<MethodBridgeSig> _managed2nativeMethodList;

        private readonly HashSet<MethodBridgeSig> _native2managedMethodSet = new HashSet<MethodBridgeSig>();

        private List<MethodBridgeSig> _native2managedMethodList;

        private readonly HashSet<MethodBridgeSig> _adjustThunkMethodSet = new HashSet<MethodBridgeSig>();

        private List<MethodBridgeSig> _adjustThunkMethodList;

        public bool IsHotFixType(Type type)
        {
            return _hotfixAssemblies.Contains(type.Assembly);
        }

        public MethodBridgeGenerator(MethodBridgeGeneratorOptions options)
        {
            _hotfixAssemblies = new HashSet<Assembly>(options.HotfixAssemblies);
            _assemblies = options.AllAssemblies;
            _callConvention = options.CallConvention;
            _outputFile = options.OutputFile;
            _platformAdaptor = CreatePlatformAdaptor(options.CallConvention);
            _optimized = options.Optimized;
        }

        private static IPlatformAdaptor CreatePlatformAdaptor(PlatformABI type)
        {
            switch (type)
            {
                case PlatformABI.Universal32: return new PlatformAdaptor_Universal32();
                case PlatformABI.Universal64: return new PlatformAdaptor_Universal64();
                case PlatformABI.Arm64: return new PlatformAdaptor_Arm64();
                default: throw new NotSupportedException();
            }
        }

        private string GetTemplateFile()
        {
            string tplFile;

            switch (_callConvention)
            {
                case PlatformABI.Universal32: tplFile = "Universal32"; break;
                case PlatformABI.Universal64: tplFile = "Universal64"; break;
                case PlatformABI.Arm64: tplFile = "Arm64"; break;
                default: throw new NotSupportedException();
            };
            return $"{Application.dataPath}/Scripts/Editor/HybridCLR/Generators/Templates/MethodBridge_{tplFile}.cpp";
        }

        public IEnumerable<TypeGenInfo> GetGenerateTypes()
        {
            return new List<TypeGenInfo>();
        }

        private MethodBridgeSig CreateMethodBridgeSig(bool isStatic, ParameterInfo returnType, ParameterInfo[] parameters)
        {
            var paramInfos = new List<ParamInfo>();
            if (!isStatic)
            {
                // FIXME arm32 is s_i4u4
                paramInfos.Add(new ParamInfo() { Type = _platformAdaptor.IsArch32 ? TypeInfo.s_i4u4 : TypeInfo.s_i8u8 });
            }
            foreach (var paramInfo in parameters)
            {
                paramInfos.Add(new ParamInfo() { Type = _platformAdaptor.CreateTypeInfo(paramInfo.ParameterType, false) });
            }
            var mbs = new MethodBridgeSig()
            {
                ReturnInfo = new ReturnInfo() { Type = returnType != null ? _platformAdaptor.CreateTypeInfo(returnType.ParameterType, true) : TypeInfo.s_void },
                ParamInfos = paramInfos,
            };
            return mbs;
        }

        private void AddManaged2NativeMethod(MethodBridgeSig method)
        {
            if (_managed2nativeMethodSet.Add(method))
            {
                method.Init();
            }
        }

        private void AddNative2ManagedMethod(MethodBridgeSig method)
        {
            if (_native2managedMethodSet.Add(method))
            {
                method.Init();
            }
        }

        private void AddAdjustThunkMethod(MethodBridgeSig method)
        {
            if (_adjustThunkMethodSet.Add(method))
            {
                method.Init();
            }
        }

        private void ScanType(Type type)
        {
            if (type.IsGenericTypeDefinition)
            {
                return;
            }
            if (_optimized)
            {
                if (!type.IsNested)
                {
                    if (!type.IsPublic)
                    {
                        return;
                    }
                }
                else
                {
                    if (type.IsNestedPrivate)
                    {
                        return;
                    }
                }
            }
            var typeDel = typeof(MulticastDelegate);
            if (typeDel.IsAssignableFrom(type))
            {
                var method = type.GetMethod("Invoke");
                if (method == null)
                {
                    //Debug.LogError($"delegate:{typeDel.FullName} Invoke not exists");
                    return;
                }
                // Debug.Log($"== delegate:{type}");
                var instanceCallMethod = CreateMethodBridgeSig(false, method.ReturnParameter, method.GetParameters());
                AddManaged2NativeMethod(instanceCallMethod);
                AddNative2ManagedMethod(instanceCallMethod);

                var staticCallMethod = CreateMethodBridgeSig(true, method.ReturnParameter, method.GetParameters());
                AddManaged2NativeMethod(staticCallMethod);
                AddNative2ManagedMethod(staticCallMethod);
                return;
            }
            foreach (var method in type.GetMethods(BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public
| BindingFlags.NonPublic | BindingFlags.CreateInstance | BindingFlags.InvokeMethod | BindingFlags.FlattenHierarchy))
            {
                if (method.IsGenericMethodDefinition)
                {
                    continue;
                }

                if (_optimized && (method.IsPrivate || (method.IsAssembly && !method.IsPublic && !method.IsFamily)))
                {
                    continue;
                }

                if (!_optimized || (method.IsFamily || method.IsPublic))
                {
                    var m2nMethod = CreateMethodBridgeSig(method.IsStatic, method.ReturnParameter, method.GetParameters());
                    AddManaged2NativeMethod(m2nMethod);

                    if (type.IsValueType && !method.IsStatic)
                    {
                        var adjustThunkMethod = CreateMethodBridgeSig(false, method.ReturnParameter, method.GetParameters());
                        AddAdjustThunkMethod(adjustThunkMethod);
                    }

                    if (method.IsVirtual)
                    {
                        AddNative2ManagedMethod(m2nMethod);
                    }
                }
            }

            foreach (var method in type.GetConstructors(BindingFlags.Instance | BindingFlags.Public
| BindingFlags.NonPublic | BindingFlags.CreateInstance | BindingFlags.InvokeMethod | BindingFlags.FlattenHierarchy))
            {
                if (_optimized && (method.IsPrivate || (method.IsAssembly && !method.IsPublic && !method.IsFamily)))
                {
                    continue;
                }

                if (!_optimized || (method.IsFamily || method.IsPublic))
                {
                    var callMethod = CreateMethodBridgeSig(false, null, method.GetParameters());
                    AddManaged2NativeMethod(callMethod);

                    if (type.IsValueType && !method.IsStatic)
                    {
                        var invokeMethod = CreateMethodBridgeSig(false, null, method.GetParameters());
                        AddAdjustThunkMethod(invokeMethod);
                    }
                }
            }

            foreach (var subType in type.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic))
            {
                ScanType(subType);
            }
        }

        public void PrepareFromAssemblies()
        {
            foreach (var ass in _assemblies)
            {
                if (_hotfixAssemblies.Contains(ass))
                {
                    continue;
                }
                //Debug.Log("prepare assembly:" + ass.FullName);
                foreach (var type in ass.GetTypes())
                {
                    ScanType(type);
                }
            }
        }

        private void PrepareMethodsFromCustomeGenericTypes()
        {
            foreach (var type in GeneratorConfig.PrepareCustomGenericTypes())
            {
                ScanType(type);
            }
        }

        public void PrepareMethods()
        {
            PrepareMethodsFromCustomeGenericTypes();


            foreach(var methodSig in _platformAdaptor.IsArch32 ? GeneratorConfig.PrepareCustomMethodSignatures32() : GeneratorConfig.PrepareCustomMethodSignatures64())
            {
                var method = MethodBridgeSig.CreateBySignatuer(methodSig);
                AddManaged2NativeMethod(method);
                AddAdjustThunkMethod(method);
            }
            PrepareFromAssemblies();

            {
                var sortedMethods = new SortedDictionary<string, MethodBridgeSig>();
                foreach (var method in _managed2nativeMethodSet)
                {
                    sortedMethods.Add(method.CreateCallSigName(), method);
                }
                _managed2nativeMethodList = sortedMethods.Values.ToList();
            }
            {
                var sortedMethods = new SortedDictionary<string, MethodBridgeSig>();
                foreach (var method in _native2managedMethodSet)
                {
                    sortedMethods.Add(method.CreateCallSigName(), method);
                }
                _native2managedMethodList = sortedMethods.Values.ToList();
            }
            {
                var sortedMethods = new SortedDictionary<string, MethodBridgeSig>();
                foreach (var method in _adjustThunkMethodSet)
                {
                    sortedMethods.Add(method.CreateCallSigName(), method);
                }
                _adjustThunkMethodList = sortedMethods.Values.ToList();
            }
        }

        public void Generate()
        {
            var frr = new FileRegionReplace(GetTemplateFile());

            List<string> lines = new List<string>(20_0000);

            Debug.LogFormat("== managed2native method count:{0}", _managed2nativeMethodList.Count);

            foreach(var method in _managed2nativeMethodList)
            {
                _platformAdaptor.GenerateManaged2NativeMethod(method, lines);
            }

            _platformAdaptor.GenerateManaged2NativeStub(_managed2nativeMethodList, lines);

            Debug.LogFormat("== native2managed method count:{0}", _native2managedMethodList.Count);

            foreach (var method in _native2managedMethodList)
            {
                _platformAdaptor.GenerateNative2ManagedMethod(method, lines);
            }

            _platformAdaptor.GenerateNative2ManagedStub(_native2managedMethodList, lines);

            Debug.LogFormat("== adjustThunk method count:{0}", _adjustThunkMethodList.Count);

            foreach (var method in _adjustThunkMethodList)
            {
                _platformAdaptor.GenerateAdjustThunkMethod(method, lines);
            }

            _platformAdaptor.GenerateAdjustThunkStub(_adjustThunkMethodList, lines);

            frr.Replace("INVOKE_STUB", string.Join("\n", lines));

            Directory.CreateDirectory(Path.GetDirectoryName(_outputFile));

            frr.Commit(_outputFile);
        }

    }
}
