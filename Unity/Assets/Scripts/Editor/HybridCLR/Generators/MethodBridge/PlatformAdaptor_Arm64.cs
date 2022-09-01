using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace HybridCLR.Editor.Generators.MethodBridge
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

        public PlatformABI CallConventionType { get; } = PlatformABI.Universal64;

        public override bool IsArch32 => false;

        public override TypeInfo PointerType => TypeInfo.s_i8u8;

        protected override Dictionary<Type, TypeInfo> CacheTypes => s_typeInfoCaches;

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

        protected override TypeInfo CreateValueType(Type type, bool returnValue)
        {
            (int typeSize, int typeAligment) = ComputeSizeAndAligmentOfArch64(type);
            if (ComputHFATypeInfo(type, typeSize, out HFATypeInfo hfaTypeInfo))
            {
                if (hfaTypeInfo.Type == typeof(float))
                {
                    switch (hfaTypeInfo.Count)
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
                // 64位下结构体内存对齐规则是一样的
                return CreateArm64GeneralValueType(type, typeSize,returnValue);
            }

        }

        private TypeInfo CreateArm64GeneralValueType(Type type, int typeSize, bool returnValue)
        {
            if (typeSize <= 8)
            {
                return TypeInfo.s_i8u8;
            }
            if (typeSize <= 16)
            {
                return TypeInfo.s_i16;
            }
            if (returnValue)
            {
                return new TypeInfo(type, ParamOrReturnType.STRUCTURE_ALIGN1, typeSize);
            }
            return TypeInfo.s_ref;
        }

        public override void GenerateManaged2NativeMethod(MethodBridgeSig method, List<string> lines)
        {
            int totalQuadWordNum = method.ParamInfos.Count + method.ReturnInfo.GetParamSlotNum(this.CallConventionType);
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
