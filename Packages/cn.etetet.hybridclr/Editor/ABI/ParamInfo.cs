using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HybridCLR.Editor.ABI
{

    public class ParamInfo
    {
        public TypeInfo Type { get; set; }

        public int Index { get; set; }

    }

    public class ReturnInfo
    {
        public TypeInfo Type { get; set; }

        public bool IsVoid => Type.PorType == ParamOrReturnType.VOID;

        public override string ToString()
        {
            return Type.GetTypeName();
        }
    }
}
