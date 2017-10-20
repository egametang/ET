using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ILRuntime.Runtime.Debugger.Protocol
{
    public class SCThreadStarted
    {
        public int ThreadHashCode { get; set; }
    }
    public class SCThreadEnded
    {
        public int ThreadHashCode { get; set; }
    }
}
