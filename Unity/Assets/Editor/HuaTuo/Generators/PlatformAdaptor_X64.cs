using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Huatuo.Generators
{
    internal class PlatformAdaptor_X64 : PlatformAdaptorBase
    {
        public CallConventionType CallConventionType { get; } = CallConventionType.X64;

        private static readonly Dictionary<Type, TypeInfo> s_typeInfoCaches = new Dictionary<Type, TypeInfo>()
        {
            { typeof(bool), new TypeInfo(typeof(bool), ParamOrReturnType.I8_U8)},
            { typeof(byte), new TypeInfo(typeof(byte), ParamOrReturnType.I8_U8)},
            { typeof(sbyte), new TypeInfo(typeof(sbyte), ParamOrReturnType.I8_U8) },
            { typeof(short), new TypeInfo(typeof(short), ParamOrReturnType.I8_U8) },
            { typeof(ushort), new TypeInfo(typeof(ushort), ParamOrReturnType.I8_U8) },
            { typeof(char), new TypeInfo(typeof(char), ParamOrReturnType.I8_U8) },
            { typeof(int), new TypeInfo(typeof(int), ParamOrReturnType.I8_U8) },
            { typeof(uint), new TypeInfo(typeof(uint), ParamOrReturnType.I8_U8) },
            { typeof(long), new TypeInfo(typeof(long), ParamOrReturnType.I8_U8) },
            { typeof(ulong), new TypeInfo(typeof(ulong), ParamOrReturnType.I8_U8)},
            { typeof(IntPtr), new TypeInfo(null, ParamOrReturnType.I8_U8)},
            { typeof(UIntPtr), new TypeInfo(null, ParamOrReturnType.I8_U8)},
            { typeof(float), new TypeInfo(typeof(float), ParamOrReturnType.R8)},
            { typeof(double), new TypeInfo(typeof(double), ParamOrReturnType.R8)},
        };

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
            int size = ComputeSizeOf(type);
            //Debug.LogFormat("type:{0} size:{1}", type, size);
            switch(size)
            {
                case 1:
                case 2:
                case 4:
                case 8: return new TypeInfo(type, ParamOrReturnType.I8_U8);
                default: return returnValue ? new TypeInfo(type, ParamOrReturnType.STRUCTURE_SIZE_GT_16, size) :
                        new TypeInfo(type, ParamOrReturnType.STRUCTURE_AS_REF_PARAM);
            }
        }

        public override IEnumerable<MethodBridgeSig> GetPreserveMethods()
        {
            yield break;
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
