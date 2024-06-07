namespace ET
{
    public static partial class TimerInvokeType
    {
        public const int SessionIdleChecker = PackageType.StateSync * 1000 + 1;
        public const int SessionAcceptTimeout = PackageType.StateSync * 1000 + 2;
    }
}