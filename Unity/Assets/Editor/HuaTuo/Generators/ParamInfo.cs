using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Huatuo.Generators
{

    public class ParamInfo
    {
        public TypeInfo Type { get; set; }

        public int Index { get; set; }

        public bool IsNative2ManagedByAddress => Type.PorType >= ParamOrReturnType.STRUCT_NOT_PASS_AS_VALUE;
        public bool IsManaged2NativeDereference => Type.PorType != ParamOrReturnType.STRUCTURE_AS_REF_PARAM;

        public int GetParamSlotNum(CallConventionType canv)
        {
            return 1;
        }

        public string Native2ManagedParamValue(CallConventionType canv)
        {
            switch(canv)
            {
                case CallConventionType.X64:
                    {
                        return IsNative2ManagedByAddress ? $"(void*)&__arg{Index}" : $"*(void**)&__arg{Index}";
                    }
                case CallConventionType.Arm64:
                    {
                        return IsNative2ManagedByAddress ? $"(void*)&__arg{Index}" : $"*(void**)&__arg{Index}";
                    }
                case CallConventionType.Arm32:
                    {
                        throw new NotImplementedException();
                    }
                default:
                    throw new NotSupportedException();
            }
        }

        public string Managed2NativeParamValue(CallConventionType canv)
        {
            return IsManaged2NativeDereference ?  $"*({Type.GetTypeName()}*)(localVarBase+argVarIndexs[{Index}])" : $"({Type.GetTypeName()})(localVarBase+argVarIndexs[{Index}])";
        }
    }

    public class ReturnInfo
    {
        public TypeInfo Type { get; set; }

        public bool IsVoid => Type.PorType == ParamOrReturnType.VOID;

        public bool PassReturnAsParam => Type.PorType == ParamOrReturnType.STRUCTURE_AS_REF_PARAM;

        public int GetParamSlotNum(CallConventionType canv)
        {
            switch(Type.PorType)
            {
                case ParamOrReturnType.VOID: return 0;
                case ParamOrReturnType.ARM64_HFA_FLOAT_3: return 2;
                case ParamOrReturnType.ARM64_HFA_FLOAT_4: return 2;
                case ParamOrReturnType.ARM64_HFA_DOUBLE_2: return 2;
                case ParamOrReturnType.ARM64_HFA_DOUBLE_3: return 3;
                case ParamOrReturnType.ARM64_HFA_DOUBLE_4: return 4;
                case ParamOrReturnType.ARM64_HVA_8:
                case ParamOrReturnType.ARM64_HVA_16: throw new NotSupportedException();
                case ParamOrReturnType.STRUCTURE_SIZE_LE_16: return 2; // size <= 16
                case ParamOrReturnType.STRUCTURE_SIZE_GT_16: return (Type.Size + 7) / 8;
                default:
                    {
                        Debug.Assert(Type.PorType < ParamOrReturnType.STRUCTURE_AS_REF_PARAM);
                        return 1;
                    }
            }
        }
    }
}
