using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Huatuo.Generators
{
    public enum CallConventionType
    {
        X64,
        Arm32,
        Arm64,
    }

    public class TypeGenInfo
    {
        public Type Type { get; set; }

        public List<MethodInfo> GenericMethods { get; set; }
    }

    public class MethodBridgeGeneratorOptions
    {
        public List<Assembly> Assemblies { get; set; }

        public CallConventionType CallConvention { get; set; }

        public string OutputFile { get; set; }
    }

    public class MethodBridgeGenerator
    {
        private readonly List<Assembly> _assemblies;

        private readonly CallConventionType _callConvention;

        private readonly string _outputFile;

        private readonly IPlatformAdaptor _platformAdaptor;

        private readonly HashSet<MethodBridgeSig> _methodSet = new HashSet<MethodBridgeSig>();

        private List<MethodBridgeSig> _methodList;

        public MethodBridgeGenerator(MethodBridgeGeneratorOptions options)
        {
            _assemblies = options.Assemblies;
            _callConvention = options.CallConvention;
            _outputFile = options.OutputFile;
            _platformAdaptor = CreatePlatformAdaptor(options.CallConvention);
        }

        private static IPlatformAdaptor CreatePlatformAdaptor(CallConventionType type)
        {
            return type switch
            {
                CallConventionType.Arm32 => new PlatformAdaptor_Arm32(),
                CallConventionType.Arm64 => new PlatformAdaptor_Arm64(),
                CallConventionType.X64 => new PlatformAdaptor_X64(),
                _ => throw new NotSupportedException(),
            };
        }

        private string GetTemplateFile()
        {
            string tplFile = _callConvention switch
            {
                CallConventionType.X64 => "x64",
                CallConventionType.Arm32 => "arm32",
                CallConventionType.Arm64 => "arm64",
                _ => throw new NotSupportedException(),
            };
            return $"{Application.dataPath}/Editor/Huatuo/Templates/MethodBridge_{tplFile}.cpp";
        }

        public IEnumerable<TypeGenInfo> GetGenerateTypes()
        {
            return new List<TypeGenInfo>();
        }

        public List<MethodBridgeSig> GetGenerateMethods()
        {
            return _methodList;
        }

        private MethodBridgeSig CreateMethodBridgeSig(bool isStatic, ParameterInfo returnType, ParameterInfo[] parameters)
        {
            var paramInfos = new List<ParamInfo>();
            if (!isStatic)
            {
                // FIXME arm32 is s_i4u4
                paramInfos.Add(new ParamInfo() { Type = TypeInfo.s_i8u8 });
            }
            foreach (var paramInfo in parameters)
            {
                paramInfos.Add(new ParamInfo() { Type = _platformAdaptor.Create(paramInfo, false) });
            }
            var mbs = new MethodBridgeSig()
            {
                ReturnInfo = new ReturnInfo() { Type = returnType != null ? _platformAdaptor.Create(returnType, true) : TypeInfo.s_void },
                ParamInfos = paramInfos,
            };
            return mbs;
        }

        private void AddMethod(MethodBridgeSig method)
        {
            if (_methodSet.Add(method))
            {
                method.Init();
            }
        }

        private void ScanType(Type type)
        {
            var typeDel = typeof(Delegate);
            if (type.IsGenericTypeDefinition)
            {
                return;
            }
            foreach (var method in type.GetMethods(BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public
| BindingFlags.NonPublic | BindingFlags.CreateInstance | BindingFlags.InvokeMethod | BindingFlags.FlattenHierarchy))
            {
                if (method.IsGenericMethodDefinition)
                {
                    continue;
                }
                var mbs = CreateMethodBridgeSig(method.IsStatic, method.ReturnParameter, method.GetParameters());
                AddMethod(mbs);
                if (typeDel.IsAssignableFrom(type) && method.Name == "Invoke")
                {

                    var mbs2 = CreateMethodBridgeSig(true, method.ReturnParameter, method.GetParameters());
                    AddMethod(mbs2);
                }
            }

            foreach (var method in type.GetConstructors(BindingFlags.Instance | BindingFlags.Public
| BindingFlags.NonPublic | BindingFlags.CreateInstance | BindingFlags.InvokeMethod | BindingFlags.FlattenHierarchy))
            {
                var mbs = CreateMethodBridgeSig(false, null, method.GetParameters());
                AddMethod(mbs);
            }

            foreach(var subType in type.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic))
            {
                ScanType(subType);
            }
        }

        public void PrepareFromAssemblies()
        {
            foreach (var ass in _assemblies)
            {
                //Debug.Log("prepare assembly:" + ass.FullName);
                foreach (var type in ass.GetTypes())
                {
                    ScanType(type);
                }
            }
        }

        public void PrepareCommon1()
        {
            // (void + int64 + float) * (int64 + float) * (0 - 20) = 120
            TypeInfo typeVoid = new TypeInfo(typeof(void), ParamOrReturnType.VOID);
            TypeInfo typeLong = new TypeInfo(typeof(long), ParamOrReturnType.I8_U8);
            TypeInfo typeDouble = new TypeInfo(typeof(double), ParamOrReturnType.R8);
            int maxParamCount = 20;
                
            foreach (var returnType in new TypeInfo[] { typeVoid, typeLong, typeDouble })
            {
                var rt = new ReturnInfo() { Type = returnType };
                foreach (var argType in new TypeInfo[] { typeLong, typeDouble })
                {
                    for (int paramCount = 0; paramCount <= maxParamCount; paramCount++)
                    {
                        var paramInfos = new List<ParamInfo>();
                        for (int i = 0; i < paramCount; i++)
                        {
                            paramInfos.Add(new ParamInfo() { Type = argType });
                        }
                        var mbs = new MethodBridgeSig() { ReturnInfo = rt, ParamInfos =  paramInfos};
                        AddMethod(mbs);
                    }
                }
            }
        }

        public void PrepareCommon2()
        {
            // (void + int64 + float) * (int64 + float + sr) ^ (0 - 4) = 363
            TypeInfo typeVoid = new TypeInfo(typeof(void), ParamOrReturnType.VOID);
            TypeInfo typeLong = new TypeInfo(typeof(long), ParamOrReturnType.I8_U8);
            TypeInfo typeDouble = new TypeInfo(typeof(double), ParamOrReturnType.R8);
            TypeInfo typeStructRef = new TypeInfo(null, ParamOrReturnType.STRUCTURE_AS_REF_PARAM);

            int maxParamCount = 4;

            var argTypes = new TypeInfo[] { typeLong, typeDouble, typeStructRef };
            int paramTypeNum = argTypes.Length;
            foreach (var returnType in new TypeInfo[] { typeVoid, typeLong, typeDouble })
            {
                var rt = new ReturnInfo() { Type = returnType };
                for(int paramCount = 1; paramCount <= maxParamCount; paramCount++)
                {
                    int totalCombinationNum = (int)Math.Pow(paramTypeNum, paramCount);

                    for (int k = 0; k < totalCombinationNum; k++)
                    {
                        var paramInfos = new List<ParamInfo>();
                        int c = k;
                        for(int i = 0; i < paramCount; i++)
                        {
                            paramInfos.Add(new ParamInfo { Type = argTypes[c % paramTypeNum] });
                            c /= paramTypeNum;
                        }
                        var mbs = new MethodBridgeSig() { ReturnInfo = rt, ParamInfos = paramInfos };
                        AddMethod(mbs);
                    }
                }
            }
        }

        private void PrepareMethodsFromCustomeGenericTypes()
        {
            foreach(var type in PrepareCustomGenericTypes())
            {
                ScanType(type);
            }
        }

        /// <summary>
        /// 暂时没有仔细扫描泛型，如果运行时发现有生成缺失，先手动在此添加类
        /// </summary>
        /// <returns></returns>
        private List<Type> PrepareCustomGenericTypes()
        {
            return new List<Type>
            {
                typeof(Action<int, string, Vector3>),
            };
        }

        /// <summary>
        /// 如果提示缺失桥接函数，将提示缺失的签名加入到下列列表是简单的做法
        /// </summary>
        /// <returns></returns>
        private List<string> PrepareCustomMethodSignatures()
        {
            return new List<string>
            {
                // "vi8i8",
                "S108i8i8",
            };
        }

        public void PrepareMethods()
        {
            PrepareCommon1();
            PrepareCommon2();
            PrepareMethodsFromCustomeGenericTypes();
            foreach(var methodSig in PrepareCustomMethodSignatures())
            {
                AddMethod(MethodBridgeSig.CreateBySignatuer(methodSig));
            }
            foreach(var method in _platformAdaptor.GetPreserveMethods())
            {
                AddMethod(method);
            }
            PrepareFromAssemblies();

            var sortedMethods = new SortedDictionary<string, MethodBridgeSig>();
            foreach(var method in _methodSet)
            {
                sortedMethods.Add(method.CreateCallSigName(), method);
            }
            _methodList = sortedMethods.Values.ToList();
        }

        public void Generate()
        {
            var frr = new FileRegionReplace(GetTemplateFile());

            List<string> lines = new List<string>(20_0000);

            Debug.LogFormat("== method count:{0}", GetGenerateMethods().Count);

            _platformAdaptor.Generate(GetGenerateMethods(), lines);

            frr.Replace("INVOKE_STUB", string.Join("\n", lines));

            Directory.CreateDirectory(Path.GetDirectoryName(_outputFile));

            frr.Commit(_outputFile);
        }

    }
}
