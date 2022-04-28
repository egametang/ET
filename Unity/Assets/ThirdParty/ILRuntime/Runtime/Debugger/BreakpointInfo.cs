using ILRuntime.Runtime.Debugger.Protocol;
using ILRuntime.Runtime.Intepreter;
#if ILRUNTIME_ENABLE_ROSYLN
using Microsoft.CodeAnalysis.CSharp.Syntax;
#endif
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
        public bool Enabled { get; set; }
        public BreakpointConditionDetails Condition { get; set; }
        public UsingInfo[] UsingInfos { get; set; }

        public bool CheckCondition(DebugService debugService, ILIntepreter intp, ref StackFrameInfo[] stackFrameInfos, ref string error)
        {
#if ILRUNTIME_ENABLE_ROSYLN
            if (Condition == null || Condition.Style == BreakpointConditionStyle.None)
                return true;
            if (Condition.ExpressionError)
            {
                error = "the expression is not conditional expression";
                return true;
            }
            stackFrameInfos = debugService.GetStackFrameInfo(intp);
            try
            {
                debugService.UsingInfosContext = UsingInfos;
                var visitor = new BreakpointConditionExpressionVisitor(debugService, intp, stackFrameInfos.Length < 1 ? null : stackFrameInfos[0].LocalVariables);
                var finalResult = visitor.Visit(Condition.ExpressionSyntax);
                if (finalResult.Value is bool)
                    return (bool)finalResult.Value;
                else // TODO:处理表达式值不是bool的报错
                {
                    error = "the expression value is not bool";
                }
            }
            catch (Exception ex)
            {
                error = ex.Message;
            }
            finally
            {
                debugService.UsingInfosContext = null;
            }
#endif
            return true;
        }
    }

    public class BreakpointConditionDetails : BreakpointCondition
    {
#if ILRUNTIME_ENABLE_ROSYLN
        public ExpressionSyntax ExpressionSyntax { get; set; }
#endif
        public bool ExpressionError { get; set; }
        public Exception Exception { get; set; }
    }
}
