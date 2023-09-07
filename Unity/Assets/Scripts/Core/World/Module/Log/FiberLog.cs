using System;
using System.Diagnostics;

namespace ET
{
    public static class FiberLog
    {
        private const int TraceLevel = 1;
        private const int DebugLevel = 2;
        private const int InfoLevel = 3;
        private const int WarningLevel = 4;
        
        
        [Conditional("DEBUG")]
        public static void Debug(this Fiber self, string msg)
        {
            if (Options.Instance.LogLevel > DebugLevel)
            {
                return;
            }
            self.Log.Debug(msg);
        }
        
        [Conditional("DEBUG")]
        public static void Trace(this Fiber self, string msg)
        {
            if (Options.Instance.LogLevel > TraceLevel)
            {
                return;
            }
            
            self.Log.Trace(msg);
        }

        public static void Info(this Fiber self, string msg)
        {
            if (Options.Instance.LogLevel > InfoLevel)
            {
                return;
            }
            self.Log.Info(msg);
        }

        public static void TraceInfo(this Fiber self, string msg)
        {
            if (Options.Instance.LogLevel > InfoLevel)
            {
                return;
            }
            self.Log.Trace(msg);
        }

        public static void Warning(this Fiber self, string msg)
        {
            if (Options.Instance.LogLevel > WarningLevel)
            {
                return;
            }

            self.Log.Warning(msg);
        }

        public static void Error(this Fiber self, string msg)
        {
            self.Log.Error(msg);
        }

        public static void Error(this Fiber self, Exception e)
        {
            self.Log.Error(e.ToString());
        }
        
        public static void Console(this Fiber self, string msg)
        {
            if (Options.Instance.Console == 1)
            {
                System.Console.WriteLine(msg);
            }
            self.Log.Debug(msg);
        }

#if DOTNET
        [Conditional("DEBUG")]
        public static void Trace(this Fiber self, ref System.Runtime.CompilerServices.DefaultInterpolatedStringHandler message)
        {
            if (Options.Instance.LogLevel > TraceLevel)
            {
                return;
            }
            self.Log.Trace(ref message);
        }
        [Conditional("DEBUG")]
        public static void Warning(this Fiber self, ref System.Runtime.CompilerServices.DefaultInterpolatedStringHandler message)
        {
            if (Options.Instance.LogLevel > WarningLevel)
            {
                return;
            }
            self.Log.Warning(ref message);
        }

        public static void Info(this Fiber self, ref System.Runtime.CompilerServices.DefaultInterpolatedStringHandler message)
        {
            if (Options.Instance.LogLevel > InfoLevel)
            {
                return;
            }
            self.Log.Info(ref message);
        }
        
        [Conditional("DEBUG")]
        public static void Debug(this Fiber self, ref System.Runtime.CompilerServices.DefaultInterpolatedStringHandler message)
        {
            if (Options.Instance.LogLevel > DebugLevel)
            {
                return;
            }
            self.Log.Debug(ref message);
        }

        public static void Error(this Fiber self, ref System.Runtime.CompilerServices.DefaultInterpolatedStringHandler message)
        {
            self.Log.Error(ref message);
        }
#endif
    }
}
