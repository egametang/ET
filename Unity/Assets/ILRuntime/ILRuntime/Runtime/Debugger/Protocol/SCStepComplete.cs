using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ILRuntime.Runtime.Debugger.Protocol
{
    public class SCStepComplete
    {
        public int ThreadHashCode { get; set; }
        public KeyValuePair<int, StackFrameInfo[]>[] StackFrame { get; set; }
    }
}
