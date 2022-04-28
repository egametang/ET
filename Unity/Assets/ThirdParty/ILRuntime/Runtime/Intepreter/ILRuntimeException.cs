using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ILRuntime.Runtime.Stack;

namespace ILRuntime.Runtime.Intepreter
{
    public class ILRuntimeException : Exception
    {
        string message;
        string stackTrace;
        string thisInfo, localInfo;
        internal ILRuntimeException(string message, ILIntepreter intepreter, CLR.Method.ILMethod method, Exception innerException = null)
            : base(message, innerException)
        
        {
            this.message = message;
            var ds = intepreter.AppDomain.DebugService;
            if (innerException is ILRuntimeException)
            {
                ILRuntimeException e = innerException as ILRuntimeException;
                stackTrace = e.stackTrace;
                thisInfo = e.thisInfo;
                localInfo = e.localInfo;
            }
            else
            {
                stackTrace = ds.GetStackTrace(intepreter);
                try
                {
                    if (method.HasThis)
                        thisInfo = ds.GetThisInfo(intepreter);
                    else
                        thisInfo = "";
                    localInfo = ds.GetLocalVariableInfo(intepreter);
                }
                catch
                {

                }
            }

            if (ds.OnILRuntimeException != null) {
                ds.OnILRuntimeException(ToString());
            }
        }

        public override string Message
        {
            get
            {
                return message + "\n" + stackTrace;
            }
        }

        public override string StackTrace
        {
            get
            {
                return stackTrace;
            }
        }

        public string ThisInfo
        {
            get { return thisInfo; }
        }

        public string LocalInfo
        {
            get
            {
                return localInfo;
            }
        }

        public override string ToString()
        {
            StringBuilder message = new StringBuilder();
            message.AppendLine(this.message);
            if (!string.IsNullOrEmpty(ThisInfo))
            {
                message.AppendLine("this:");
                message.AppendLine(ThisInfo);
            }
            message.AppendLine("Local Variables:");
            message.AppendLine(LocalInfo);
            message.AppendLine(stackTrace);
            if (InnerException != null)
                message.AppendLine(InnerException.ToString());
            return message.ToString();
        }
    }
}
