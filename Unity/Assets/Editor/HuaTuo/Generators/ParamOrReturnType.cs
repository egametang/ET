using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Huatuo.Generators
{
    public enum ParamOrReturnType
    {
        VOID,
        I1_U1,
        //U1,
        I2_U2,
        //U2,
        I4_U4,
        I8_U8,
        //I_U,
        R4,
        R8,
        OBJECT_PTR_REF,
        ARM64_HFA_FLOAT_2,
        STRUCTURE_AS_REF_PARAM, // size > 16
        STRUCT_NOT_PASS_AS_VALUE, // struct  pass not as value
        ARM64_HFA_FLOAT_3,
        ARM64_HFA_FLOAT_4,
        ARM64_HFA_DOUBLE_2,
        ARM64_HFA_DOUBLE_3,
        ARM64_HFA_DOUBLE_4,
        ARM64_HVA_8,
        ARM64_HVA_16,
        STRUCTURE_SIZE_LE_16, // size <= 16
        STRUCTURE_SIZE_GT_16, // size > 16
    }
}
