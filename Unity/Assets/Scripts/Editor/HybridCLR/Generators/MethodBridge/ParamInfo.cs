using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HybridCLR.Editor.Generators.MethodBridge
{

    public class ParamInfo
    {
        public TypeInfo Type { get; set; }

        public int Index { get; set; }

        //public bool IsNative2ManagedByAddress => Type.PorType >= ParamOrReturnType.STRUCT_NOT_PASS_AS_VALUE;
        public bool IsPassToManagedByAddress => Type.GetParamSlotNum() > 1;

        public bool IsPassToNativeByAddress => Type.PorType == ParamOrReturnType.STRUCTURE_AS_REF_PARAM;

        public string Native2ManagedParamValue(PlatformABI canv)
        {
            return IsPassToManagedByAddress ? $"(uint64_t)&__arg{Index}" : $"*(uint64_t*)&__arg{Index}";
        }

        public string Managed2NativeParamValue(PlatformABI canv)
        {
            return IsPassToNativeByAddress ? $"(uint64_t)(localVarBase+argVarIndexs[{Index}])" : $"*({Type.GetTypeName()}*)(localVarBase+argVarIndexs[{Index}])";
        }
    }

    public class ReturnInfo
    {
        public TypeInfo Type { get; set; }

        public bool IsVoid => Type.PorType == ParamOrReturnType.VOID;

        public int GetParamSlotNum(PlatformABI canv)
        {
            return Type.GetParamSlotNum();
        }
    }
}
