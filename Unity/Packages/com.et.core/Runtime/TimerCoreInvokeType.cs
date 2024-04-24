namespace ET
{
    [UniqueId(0, 100)]
    public static class TimerCoreInvokeType
    {
        public const int CoroutineTimeout = 1;
        public const int SessionAcceptTimeout = 2;
        public const int SessionIdleChecker = 3;
    }
}