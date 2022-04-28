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
        public string NamespaceName { get; set; }
        public string TypeName { get; set; }
        public string MethodName { get; set; }
        public int StartLine { get; set; }
        public int EndLine { get; set; }
        public bool Enabled { get; set; }
        public BreakpointCondition Condition { get; set; }
        public UsingInfo[] UsingInfos { get; set; }
    }

    public enum BreakpointConditionStyle
    {
        None,
        WhenTrue,
        WhenChanged,
    }

    public class BreakpointCondition
    {
        public BreakpointConditionStyle Style { get; set; }
        public string Expression { get; set; }
    }

    /// <summary>
    /// 表示当前断点所在的代码段using了哪些命名空间
    /// </summary>
    public class UsingInfo
    {
        /// <summary>
        /// 表示命名空间的别名。例如"using ManagedThread = System.Threading.Thread"里面的"ManagedThread"就是别名;
        /// </summary>
        public string Alias { get; set; }
        public string Name { get; set; }
    }
}
