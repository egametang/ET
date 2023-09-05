using System;

namespace ET
{
    public interface ILog
    {
        void Trace(string message);
        void Warning(string message);
        void Info(string message);
        void Debug(string message);
        void Error(string message);
        void Error(Exception e);

#if DOTNET
        public void Trace(ref System.Runtime.CompilerServices.DefaultInterpolatedStringHandler message);
        public void Warning(ref System.Runtime.CompilerServices.DefaultInterpolatedStringHandler message);
        public void Info(ref System.Runtime.CompilerServices.DefaultInterpolatedStringHandler message);
        public void Debug(ref System.Runtime.CompilerServices.DefaultInterpolatedStringHandler message);
        public void Error(ref System.Runtime.CompilerServices.DefaultInterpolatedStringHandler message);
#endif
    }
}