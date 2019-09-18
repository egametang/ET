using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ILRuntime.Runtime.Debugger.Protocol
{
    public enum BindBreakpointResults
    {
        OK,
        TypeNotFound,
        CodeNotFound,
    }
    public class SCBindBreakpointResult
    {
        public int BreakpointHashCode { get; set; }
        public BindBreakpointResults Result { get; set; }
    }
}
