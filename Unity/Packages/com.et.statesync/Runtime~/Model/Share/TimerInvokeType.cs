namespace ET
{
    public static partial class TimerInvokeType
    {
        // 框架层100-200，逻辑层的timer type从200起
        public const int SessionIdleChecker = 101;
        
        // 框架层100-200，逻辑层的timer type 200-300
        public const int SessionAcceptTimeout = 203;
    }
}