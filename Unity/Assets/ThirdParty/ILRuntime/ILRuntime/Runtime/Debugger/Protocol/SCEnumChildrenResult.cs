using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ILRuntime.Runtime.Debugger.Protocol
{
    public class SCEnumChildrenResult
    {
        public VariableInfo[] Children { get; set; }
    }
}
