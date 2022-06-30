using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Huatuo.Generators
{
    internal class PlatformAdaptor_Arm64 : PlatformAdaptorBase
    {

        private static readonly Dictionary<Type, TypeInfo> s_typeInfoCaches = new Dictionary<Type, TypeInfo>()
        {
            { typeof(void), new TypeInfo(typeof(void), ParamOrReturnType.VOID)},
            { typeof(bool), new TypeInfo(typeof(bool), ParamOrReturnType.I1_U1)},
            { typeof(byte), new TypeInfo(typeof(byte), ParamOrReturnType.I1_U1)},
            { typeof(sbyte), new TypeInfo(typeof(sbyte), ParamOrReturnType.I1_U1) },
            { typeof(short), new TypeInfo(typeof(short), ParamOrReturnType.I2_U2) },
            { typeof(ushort), new TypeInfo(typeof(ushort), ParamOrReturnType.I2_U2) },
            { typeof(char), new TypeInfo(typeof(char), ParamOrReturnType.I2_U2) },
            { typeof(int), new TypeInfo(typeof(int), ParamOrReturnType.I4_U4) },
            { typeof(uint), new TypeInfo(typeof(uint), ParamOrReturnType.I4_U4) },
            { typeof(long), new TypeInfo(typeof(long), ParamOrReturnType.I8_U8) },
            { typeof(ulong), new TypeInfo(typeof(ulong), ParamOrReturnType.I8_U8)},
            { typeof(float), new TypeInfo(typeof(float), ParamOrReturnType.R4)},
            { typeof(double), new TypeInfo(typeof(double), ParamOrReturnType.R8)},
            { typeof(IntPtr), new TypeInfo(null, ParamOrReturnType.I8_U8)},
            { typeof(UIntPtr), new TypeInfo(null, ParamOrReturnType.I8_U8)},
            { typeof(Vector2), new TypeInfo(typeof(Vector2), ParamOrReturnType.ARM64_HFA_FLOAT_2) },
            { typeof(Vector3), new TypeInfo(typeof(Vector3), ParamOrReturnType.ARM64_HFA_FLOAT_3) },
            { typeof(Vector4), new TypeInfo(typeof(Vector4), ParamOrReturnType.ARM64_HFA_FLOAT_4) },
            { typeof(System.Numerics.Vector2), new TypeInfo(typeof(System.Numerics.Vector2), ParamOrReturnType.ARM64_HFA_FLOAT_2) },
            { typeof(System.Numerics.Vector3), new TypeInfo(typeof(System.Numerics.Vector3), ParamOrReturnType.ARM64_HFA_FLOAT_3) },
            { typeof(System.Numerics.Vector4), new TypeInfo(typeof(System.Numerics.Vector4), ParamOrReturnType.ARM64_HFA_FLOAT_4) },
        };

        public CallConventionType CallConventionType { get; } = CallConventionType.Arm64;

        public override TypeInfo Create(ParameterInfo param, bool returnValue)
        {
            var type = param.ParameterType;
            if (type.IsByRef)
            {
                return TypeInfo.s_i8u8;
            }
            if (type == typeof(void))
            {
                return TypeInfo.s_void;
            }
            if (!type.IsValueType)
            {
                return TypeInfo.s_i8u8;
            }
            if (s_typeInfoCaches.TryGetValue(type, out var cache))
            {
                return cache;
            }
            var ti = CreateValueType(type, returnValue);
            // s_typeInfoCaches.Add(type, ti);
            return ti;
        }

        private static TypeInfo CreateNormalValueTypeBySize(Type type, int typeSize, bool returnValue)
        {
            if (typeSize <= 8)
            {
                return new TypeInfo(type, ParamOrReturnType.I8_U8);
            }
            if (typeSize <= 16)
            {
                return new TypeInfo(type, ParamOrReturnType.STRUCTURE_SIZE_LE_16);
            }
            else if(returnValue)
            {
                return new TypeInfo(type, ParamOrReturnType.STRUCTURE_SIZE_GT_16, typeSize);
            }
            else
            {
                return TypeInfo.s_valueTypeAsParam;
            }
        }

        public class HFATypeInfo
        {
            public Type Type { get; set; }

            public int Count { get; set; }
        }

        private static bool IsNotHFAFastCheck(int typeSize)
        {
            return typeSize != 8 && typeSize != 12 && typeSize != 16 && typeSize != 24 && typeSize != 32;
        }

        private static bool ComputHFATypeInfo0(Type type, HFATypeInfo typeInfo)
        {
            var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var field in fields)
            {
                Type ftype = field.FieldType;
                if (ftype != typeof(float) && ftype != typeof(double))
                {
                    if (!ftype.IsPrimitive && ftype.IsValueType)
                    {
                        if (!ComputHFATypeInfo0(ftype, typeInfo))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
                else if (ftype == typeInfo.Type || typeInfo.Type == null)
                {
                    typeInfo.Type = ftype;
                    ++typeInfo.Count;
                }
                else
                {
                    return false;
                }
            }
            return typeInfo.Count <= 4;
        }

        public static bool ComputHFATypeInfo(Type type, int typeSize, out HFATypeInfo typeInfo)
        {
            typeInfo = new HFATypeInfo();
            if (IsNotHFAFastCheck(typeSize))
            {
                return false;
            }
            bool ok = ComputHFATypeInfo0(type, typeInfo);
            if (ok && typeInfo.Count >= 2 && typeInfo.Count <= 4)
            {
                int fieldSize = typeInfo.Type == typeof(float) ? 4 : 8;
                return typeSize == fieldSize * typeInfo.Count;
            }
            return false;
        }

        public static TypeInfo CreateValueType(Type type, bool returnValue)
        {
            int typeSize = ComputeSizeOf(type);
            if (ComputHFATypeInfo(type, typeSize, out HFATypeInfo hfaTypeInfo))
            {
                if (hfaTypeInfo.Type == typeof(float))
                {
                    switch(hfaTypeInfo.Count)
                    {
                        case 2: return new TypeInfo(type, ParamOrReturnType.ARM64_HFA_FLOAT_2);
                        case 3: return new TypeInfo(type, ParamOrReturnType.ARM64_HFA_FLOAT_3);
                        case 4: return new TypeInfo(type, ParamOrReturnType.ARM64_HFA_FLOAT_4);
                        default: throw new NotSupportedException();
                    }
                }
                else
                {
                    Debug.Assert(hfaTypeInfo.Type == typeof(double));
                    switch (hfaTypeInfo.Count)
                    {
                        case 2: return new TypeInfo(type, ParamOrReturnType.ARM64_HFA_DOUBLE_2);
                        case 3: return new TypeInfo(type, ParamOrReturnType.ARM64_HFA_DOUBLE_3);
                        case 4: return new TypeInfo(type, ParamOrReturnType.ARM64_HFA_DOUBLE_4);
                        default: throw new NotSupportedException();
                    }
                }
            }
            else
            {
                return CreateNormalValueTypeBySize(type, typeSize, returnValue);
            }

        }
        public IEnumerable<MethodBridgeSig> PrepareCommon1()
        {
            // (void + int32 + int64 + float + double) * (int32 + int64 + float + double) * (0 - 20) = 420
            TypeInfo typeVoid = new TypeInfo(typeof(void), ParamOrReturnType.VOID);
            TypeInfo typeInt = new TypeInfo(typeof(int), ParamOrReturnType.I4_U4);
            TypeInfo typeLong = new TypeInfo(typeof(long), ParamOrReturnType.I8_U8);
            TypeInfo typeFloat = new TypeInfo(typeof(float), ParamOrReturnType.R4);
            TypeInfo typeDouble = new TypeInfo(typeof(double), ParamOrReturnType.R8);
            int maxParamCount = 20;

            foreach (var returnType in new TypeInfo[] { typeVoid, typeInt, typeLong, typeFloat, typeDouble })
            {
                var rt = new ReturnInfo() { Type = returnType };
                foreach (var argType in new TypeInfo[] { typeInt, typeLong, typeFloat, typeDouble })
                {
                    for (int paramCount = 0; paramCount <= maxParamCount; paramCount++)
                    {
                        var paramInfos = new List<ParamInfo>();
                        for (int i = 0; i < paramCount; i++)
                        {
                            paramInfos.Add(new ParamInfo() { Type = argType });
                        }
                        var mbs = new MethodBridgeSig() { ReturnInfo = rt, ParamInfos = paramInfos };
                        yield return mbs;
                    }
                }
            }
        }

        public IEnumerable<MethodBridgeSig> PrepareCommon2()
        {
            // (void + int32 + int64 + float + double + v2f + v3f + v4f + s2) * (int32 + int64 + float + double + v2f + v3f + v4f + s2 + sr) ^ (0 - 2) = 399
            TypeInfo typeVoid = new TypeInfo(typeof(void), ParamOrReturnType.VOID);
            TypeInfo typeInt = new TypeInfo(typeof(int), ParamOrReturnType.I4_U4);
            TypeInfo typeLong = new TypeInfo(typeof(long), ParamOrReturnType.I8_U8);
            TypeInfo typeFloat = new TypeInfo(typeof(float), ParamOrReturnType.R4);
            TypeInfo typeDouble = new TypeInfo(typeof(double), ParamOrReturnType.R8);
            TypeInfo typeV2f = new TypeInfo(typeof(Vector2), ParamOrReturnType.ARM64_HFA_FLOAT_2);
            TypeInfo typeV3f = new TypeInfo(typeof(Vector3), ParamOrReturnType.ARM64_HFA_FLOAT_3);
            TypeInfo typeV4f = new TypeInfo(typeof(Vector4), ParamOrReturnType.ARM64_HFA_FLOAT_4);
            TypeInfo typeStructLe16 = new TypeInfo(null, ParamOrReturnType.STRUCTURE_SIZE_LE_16);
            TypeInfo typeStructRef = new TypeInfo(null, ParamOrReturnType.STRUCTURE_AS_REF_PARAM);

            int maxParamCount = 2;

            var argTypes = new TypeInfo[] { typeInt, typeLong, typeFloat, typeDouble, typeV2f, typeV3f, typeV4f, typeStructLe16, typeStructRef };
            int paramTypeNum = argTypes.Length;
            foreach (var returnType in new TypeInfo[] { typeVoid, typeInt, typeLong, typeFloat, typeDouble, typeV2f, typeV3f, typeV4f, typeStructRef })
            {
                var rt = new ReturnInfo() { Type = returnType };
                for (int paramCount = 0; paramCount <= maxParamCount; paramCount++)
                {
                    int totalCombinationNum = (int)Math.Pow(paramTypeNum, paramCount);

                    for (int k = 0; k < totalCombinationNum; k++)
                    {
                        var paramInfos = new List<ParamInfo>();
                        int c = k;
                        for (int i = 0; i < paramCount; i++)
                        {
                            paramInfos.Add(new ParamInfo { Type = argTypes[c % paramTypeNum] });
                            c /= paramTypeNum;
                        }
                        var mbs = new MethodBridgeSig() { ReturnInfo = rt, ParamInfos = paramInfos };
                        yield return mbs;
                    }
                }
            }
        }

        public override IEnumerable<MethodBridgeSig> GetPreserveMethods()
        {
            foreach (var method in PrepareCommon1())
            {
                yield return method;
            }
            foreach (var method in PrepareCommon2())
            {
                yield return method;
            }
        }

        protected override void GenMethod(MethodBridgeSig method, List<string> lines)
        {
            //int totalQuadWordNum = method.ParamInfos.Sum(p => p.GetParamSlotNum(this.CallConventionType)) + method.ReturnInfo.GetParamSlotNum(this.CallConventionType);
            int totalQuadWordNum = method.ParamInfos.Count + method.ReturnInfo.GetParamSlotNum(this.CallConventionType);



            string paramListStr = string.Join(", ", method.ParamInfos.Select(p => $"{p.Type.GetTypeName()} __arg{p.Index}").Concat(new string[] { "const MethodInfo* method" }));
            string paramTypeListStr = string.Join(", ", method.ParamInfos.Select(p => $"{p.Type.GetTypeName()}").Concat(new string[] { "const MethodInfo*" })); ;
            string paramNameListStr = string.Join(", ", method.ParamInfos.Select(p => p.Managed2NativeParamValue(this.CallConventionType)).Concat(new string[] { "method" }));

            string invokeAssignArgs = @$"
	if (huatuo::IsInstanceMethod(method))
	{{
        args[0].ptr = __this;
{string.Join("\n", method.ParamInfos.Skip(1).Select(p => $"\t\targs[{p.Index}].u64 = *(uint64_t*)__args[{p.Index - 1}];"))}
    }}
	else
	{{
{string.Join("\n", method.ParamInfos.Select(p => $"\t\targs[{p.Index}].u64 = *(uint64_t*)__args[{p.Index}];"))}
    }}
";

            {
                lines.Add($@"
static {method.ReturnInfo.Type.GetTypeName()} __Native2ManagedCall_{method.CreateCallSigName()}({paramListStr})
{{
    StackObject args[{Math.Max(totalQuadWordNum, 1)}] = {{{string.Join(", ", method.ParamInfos.Select(p => p.Native2ManagedParamValue(this.CallConventionType)))} }};
    StackObject* ret = {(method.ReturnInfo.IsVoid ? "nullptr" : "args + " + method.ParamInfos.Count)};
    Interpreter::Execute(method, args, ret);
    {(!method.ReturnInfo.IsVoid ? $"return *({method.ReturnInfo.Type.GetTypeName()}*)ret;" : "")}
}}

static {method.ReturnInfo.Type.GetTypeName()} __Native2ManagedCall_AdjustorThunk_{method.CreateCallSigName()}({paramListStr})
{{
    StackObject args[{Math.Max(totalQuadWordNum, 1)}] = {{{string.Join(", ", method.ParamInfos.Select(p => (p.Index == 0 ? $"*(uint8_t**)&__arg{p.Index} + sizeof(Il2CppObject)" : p.Native2ManagedParamValue(this.CallConventionType))))} }};
    StackObject* ret = {(method.ReturnInfo.IsVoid ? "nullptr" : "args + " + method.ParamInfos.Count)};
    Interpreter::Execute(method, args, ret);
    {(!method.ReturnInfo.IsVoid ? $"return *({method.ReturnInfo.Type.GetTypeName()}*)ret;" : "")}
}}

static void __Managed2NativeCall_{method.CreateCallSigName()}(const MethodInfo* method, uint16_t* argVarIndexs, StackObject* localVarBase, void* ret)
{{
    if (huatuo::metadata::IsInstanceMethod(method) && !localVarBase[argVarIndexs[0]].obj)
    {{
        il2cpp::vm::Exception::RaiseNullReferenceException();
    }}
    Interpreter::RuntimeClassCCtorInit(method);
    typedef {method.ReturnInfo.Type.GetTypeName()} (*NativeMethod)({paramListStr});
    {(!method.ReturnInfo.IsVoid ? $"*({method.ReturnInfo.Type.GetTypeName()}*)ret = " : "")}((NativeMethod)(method->methodPointer))({paramNameListStr});
}}
");
            }

            lines.Add($@"
#ifdef HUATUO_UNITY_2021_OR_NEW
static void __Invoke_instance_{method.CreateCallSigName()}(Il2CppMethodPointer __methodPtr, const MethodInfo* __method, void* __this, void** __args, void* __ret)
{{
    StackObject args[{totalQuadWordNum + 1}] = {{ AdjustValueTypeSelfPointer(({ConstStrings.typeObjectPtr})__this, __method)}};
    ConvertInvokeArgs(args+1, __method, __args);
    Interpreter::Execute(__method, args, __ret);
}}

static void __Invoke_static_{method.CreateCallSigName()}(Il2CppMethodPointer __methodPtr, const MethodInfo* __method, void* __this, void** __args, void* __ret)
{{
    StackObject args[{Math.Max(totalQuadWordNum, 1)}] = {{ }};
    ConvertInvokeArgs(args, __method, __args);
    Interpreter::Execute(__method, args, __ret);
}}
#else
static void* __Invoke_instance_{method.CreateCallSigName()}(Il2CppMethodPointer __methodPtr, const MethodInfo* __method, void* __this, void** __args)
{{
    StackObject args[{totalQuadWordNum + 1}] = {{ AdjustValueTypeSelfPointer(({ConstStrings.typeObjectPtr})__this, __method)}};
    ConvertInvokeArgs(args+1, __method, __args);
    StackObject* ret = {(!method.ReturnInfo.IsVoid ? "args + " + (method.ParamInfos.Count + 1) : "nullptr")};
    Interpreter::Execute(__method, args, ret);
    return {(!method.ReturnInfo.IsVoid ? $"TranslateNativeValueToBoxValue(__method->return_type, ret)" : "nullptr")};
}}

static void* __Invoke_static_{method.CreateCallSigName()}(Il2CppMethodPointer __methodPtr, const MethodInfo* __method, void* __this, void** __args)
{{
    StackObject args[{Math.Max(totalQuadWordNum, 1)}] = {{ }};
    ConvertInvokeArgs(args, __method, __args);
    StackObject* ret = {(!method.ReturnInfo.IsVoid ? "args + " + method.ParamInfos.Count : "nullptr")};
    Interpreter::Execute(__method, args, ret);
    return {(!method.ReturnInfo.IsVoid ? $"TranslateNativeValueToBoxValue(__method->return_type, ret)" : "nullptr")};
}}
#endif
");
        }
    }
}
