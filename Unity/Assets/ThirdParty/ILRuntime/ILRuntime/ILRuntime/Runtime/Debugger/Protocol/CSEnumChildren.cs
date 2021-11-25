using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ILRuntime.Runtime.Debugger.Protocol
{
    public class CSEnumChildren
    {
        public int ThreadHashCode { get; set; }
        public VariableReference Parent { get; set; }
    }
}
