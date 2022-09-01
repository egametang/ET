using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace HybridCLR.Editor.Generators.MethodBridge
{
    internal class PlatformAdaptor_Universal32 : PlatformAdaptorBase
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
            { typeof(IntPtr), new TypeInfo(null, ParamOrReturnType.I4_U4)},
            { typeof(UIntPtr), new TypeInfo(null, ParamOrReturnType.I4_U4)},
        };


        public PlatformABI CallConventionType { get; } = PlatformABI.Universal32;

        public override bool IsArch32 => true;

        public override TypeInfo PointerType => TypeInfo.s_i4u4;

        protected override Dictionary<Type, TypeInfo> CacheTypes => s_typeInfoCaches;

        protected override TypeInfo CreateValueType(Type type, bool returnValue)
        {
            (int typeSize, int typeAligment) = ComputeSizeAndAligmentOfArch32(type);
            int actualAliment = typeAligment <= 4 ? 1 : 8;
            return CreateGeneralValueType(type, typeSize, actualAliment);
        }

        public override void GenerateManaged2NativeMethod(MethodBridgeSig method, List<string> lines)
        {
            string paramListStr = string.Join(", ", method.ParamInfos.Select(p => $"{p.Type.GetTypeName()} __arg{p.Index}").Concat(new string[] { "const MethodInfo* method" }));
            string paramNameListStr = string.Join(", ", method.ParamInfos.Select(p => p.Managed2NativeParamValue(this.CallConventionType)).Concat(new string[] { "method" }));

            lines.Add($@"
static void __M2N_{method.CreateCallSigName()}(const MethodInfo* method, uint16_t* argVarIndexs, StackObject* localVarBase, void* ret)
{{
    typedef {method.ReturnInfo.Type.GetTypeName()} (*NativeMethod)({paramListStr});
    {(!method.ReturnInfo.IsVoid ? $"*({method.ReturnInfo.Type.GetTypeName()}*)ret = " : "")}((NativeMethod)(GetInterpreterDirectlyCallMethodPointer(method)))({paramNameListStr});
}}
");
        }
        public override void GenerateNative2ManagedMethod(MethodBridgeSig method, List<string> lines)
        {
            int totalQuadWordNum = method.ParamInfos.Count + method.ReturnInfo.GetParamSlotNum(this.CallConventionType);
            string paramListStr = string.Join(", ", method.ParamInfos.Select(p => $"{p.Type.GetTypeName()} __arg{p.Index}").Concat(new string[] { "const MethodInfo* method" }));
            
            lines.Add($@"
static {method.ReturnInfo.Type.GetTypeName()} __N2M_{method.CreateCallSigName()}({paramListStr})
{{
    StackObject args[{Math.Max(totalQuadWordNum, 1)}] = {{{string.Join(", ", method.ParamInfos.Select(p => p.Native2ManagedParamValue(this.CallConventionType)))} }};
    StackObject* ret = {(method.ReturnInfo.IsVoid ? "nullptr" : "args + " + method.ParamInfos.Count)};
    Interpreter::Execute(method, args, ret);
    {(!method.ReturnInfo.IsVoid ? $"return *({method.ReturnInfo.Type.GetTypeName()}*)ret;" : "")}
}}
");
        }
        public override void GenerateAdjustThunkMethod(MethodBridgeSig method, List<string> lines)
        {
            int totalQuadWordNum = method.ParamInfos.Count + method.ReturnInfo.GetParamSlotNum(this.CallConventionType);

            string paramListStr = string.Join(", ", method.ParamInfos.Select(p => $"{p.Type.GetTypeName()} __arg{p.Index}").Concat(new string[] { "const MethodInfo* method" }));

            lines.Add($@"
static {method.ReturnInfo.Type.GetTypeName()} __N2M_AdjustorThunk_{method.CreateCallSigName()}({paramListStr})
{{
    StackObject args[{Math.Max(totalQuadWordNum, 1)}] = {{{string.Join(", ", method.ParamInfos.Select(p => (p.Index == 0 ? $"(uint64_t)(*(uint8_t**)&__arg{p.Index} + sizeof(Il2CppObject))" : p.Native2ManagedParamValue(this.CallConventionType))))} }};
    StackObject* ret = {(method.ReturnInfo.IsVoid ? "nullptr" : "args + " + method.ParamInfos.Count)};
    Interpreter::Execute(method, args, ret);
    {(!method.ReturnInfo.IsVoid ? $"return *({method.ReturnInfo.Type.GetTypeName()}*)ret;" : "")}
}}
");

        }
    }

}
