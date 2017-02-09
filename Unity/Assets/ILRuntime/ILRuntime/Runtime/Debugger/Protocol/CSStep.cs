using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ILRuntime.Runtime.Debugger.Protocol
{
    public class CSStep
    {
        public int ThreadHashCode { get; set; }
        public StepTypes StepType { get; set; }
    }
}
