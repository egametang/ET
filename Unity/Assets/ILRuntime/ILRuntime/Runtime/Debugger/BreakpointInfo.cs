using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ILRuntime.Runtime.Debugger
{
    class BreakpointInfo
    {
        public int BreakpointHashCode { get; set; }
        public int MethodHashCode { get; set; }
        public int StartLine { get; set; }
    }
}
