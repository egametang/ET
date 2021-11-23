using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ILRuntime.Runtime.Debugger.Protocol
{
    public class SCBreakpointHit
    {
        public int BreakpointHashCode { get; set; }
        public int ThreadHashCode { get; set; }
        public KeyValuePair<int, StackFrameInfo[]>[] StackFrame { get; set; }
    }
}
