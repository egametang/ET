namespace ET
{
    public static partial class TimerInvokeType
    {
        public const int SessionIdleChecker = PackageType.Login * 1000 + 1;
        public const int SessionAcceptTimeout = PackageType.Login * 1000 + 2;
    }
}