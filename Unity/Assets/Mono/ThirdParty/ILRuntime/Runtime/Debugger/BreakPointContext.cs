using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ILRuntime.Runtime.Intepreter;
using ILRuntime.Runtime.Stack;
namespace ILRuntime.Runtime.Debugger
{
    unsafe class BreakPointContext
    {
        public ILIntepreter Interpreter { get; set; }
        public Exception Exception { get; set; }

        public string DumpContext()
        {
            /*StringBuilder sb = new StringBuilder();
            if (Exception != null)
                sb.AppendLine(Exception.Message);
            StackFrame[] frames = Interpreter.Stack.Frames.ToArray();
            StackFrame topFrame = frames[0];
            var m = topFrame.Method;
            if (m.HasThis)
            {
                sb.AppendLine("this:");
                sb.AppendLine(DebugService.Instance.GetThisInfo(Interpreter));
            }
            sb.AppendLine("->" + topFrame.Method.Definition.Body.Instructions[topFrame.Address.Value]);
            sb.AppendLine("Local Variables:");
            sb.AppendLine(DebugService.Instance.GetLocalVariableInfo(Interpreter));

            sb.Append(DebugService.Instance.GetStackTrance(Interpreter));
            return sb.ToString();*/
            return null;
        }

        string GetStackObjectValue(StackObject val, IList<object> mStack)
        {
            string v;
            switch (val.ObjectType)
            {
                case ObjectTypes.Null:
                    v = "null";
                    break;
                case ObjectTypes.Integer:
                    v = val.Value.ToString();
                    break;
                case ObjectTypes.Object:
                    {
                        object obj = Interpreter.Stack.ManagedStack[val.Value];
                        v = obj.ToString();
                    }
                    break;
                default:
                    v = "Unknown type";
                    break;
            }
            return v;
        }        
    }
}
