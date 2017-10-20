using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ILRuntime.Runtime.Debugger
{
    public class StackFrameInfo
    {
        public string MethodName { get; set; }
        public string DocumentName { get; set; }
        public int StartLine { get; set; }
        public int StartColumn { get; set; }
        public int EndLine { get; set; }
        public int EndColumn { get; set; }
        public VariableInfo[] LocalVariables { get; set; }
    }
}
