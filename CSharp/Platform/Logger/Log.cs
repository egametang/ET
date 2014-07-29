namespace Logger
{
    public static class Log
    {
        private static readonly ILog globalLog = new NLogAdapter(new StackInfoDecorater());

        public static ILog GlobalLog
        {
            get
            {
                return globalLog;
            }
        }

        public static void Trace(string message)
        {
            GlobalLog.Trace(message);
        }

        public static void Trace(string format, params object[] args)
        {
            GlobalLog.Trace(string.Format(format, args));
        }

        public static void Debug(string format)
        {
            GlobalLog.Debug(format);
        }

        public static void Debug(string format, params object[] args)
        {
            GlobalLog.Debug(string.Format(format, args));
        }
    }
}