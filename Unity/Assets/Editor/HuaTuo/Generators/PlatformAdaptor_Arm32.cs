using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Huatuo.Generators
{
    internal class PlatformAdaptor_Arm32 : PlatformAdaptorBase
    {


        private static readonly TypeInfo s_objetPtrRef = new TypeInfo(null, ParamOrReturnType.OBJECT_PTR_REF);
        private static readonly TypeInfo s_void = new TypeInfo(typeof(void), ParamOrReturnType.VOID);

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
            { typeof(IntPtr), new TypeInfo(null, ParamOrReturnType.OBJECT_PTR_REF)},
            { typeof(UIntPtr), new TypeInfo(null, ParamOrReturnType.OBJECT_PTR_REF)},
            { typeof(Vector2), new TypeInfo(typeof(Vector2), ParamOrReturnType.ARM64_HFA_FLOAT_2) },
            { typeof(System.Numerics.Vector2), new TypeInfo(typeof(System.Numerics.Vector2), ParamOrReturnType.ARM64_HFA_FLOAT_2) },
            { typeof(Vector3), new TypeInfo(typeof(Vector3), ParamOrReturnType.ARM64_HFA_FLOAT_3) },
            { typeof(System.Numerics.Vector3), new TypeInfo(typeof(System.Numerics.Vector3), ParamOrReturnType.ARM64_HFA_FLOAT_3) },
            { typeof(Vector4), new TypeInfo(typeof(Vector4), ParamOrReturnType.ARM64_HFA_FLOAT_4) },
            { typeof(System.Numerics.Vector4), new TypeInfo(typeof(System.Numerics.Vector4), ParamOrReturnType.ARM64_HFA_FLOAT_4) },
        };


        public override TypeInfo Create(ParameterInfo param, bool returnValue)
        {
            var type = param.ParameterType;
            if (type.IsByRef)
            {
                return s_objetPtrRef;
            }
            if (type == typeof(void))
            {
                return s_void;
            }
            if (!type.IsValueType)
            {
                return s_objetPtrRef;
            }
            if (s_typeInfoCaches.TryGetValue(type, out var cache))
            {
                return cache;
            }
            return null;
        }

        public override IEnumerable<MethodBridgeSig> GetPreserveMethods()
        {
            yield break;
        }

        protected override void GenMethod(MethodBridgeSig method, List<string> lines)
        {
            throw new NotImplementedException();
        }
    }
}
