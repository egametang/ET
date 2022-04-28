using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ILRuntime.Runtime.Debugger.Protocol
{
    public class CSResolveIndexer
    {
        public int ThreadHashCode { get; set; }
        public int FrameIndex { get; set; }
        public VariableReference Index { get; set; }
        public VariableReference Body { get; set; }
    }
}
