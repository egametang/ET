using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ILRuntime.Runtime.Debugger.Protocol
{
    public class CSBindBreakpoint
    {
        public int BreakpointHashCode { get; set; }
        public bool IsLambda { get; set; }
        public string TypeName { get; set; }
        public string MethodName { get; set; }
        public int StartLine { get; set; }
        public int EndLine { get; set; }
    }
}
