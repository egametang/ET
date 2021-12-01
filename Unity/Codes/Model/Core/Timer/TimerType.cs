namespace ET
{
    public static partial class TimerType
    {
        // 框架层0-1000，逻辑层的timer type从1000起
        public const int WaitTimer = 0;
        public const int SessionIdleChecker = 1;
        public const int ActorLocationSenderChecker = 2;
        public const int ActorMessageSenderChecker = 3;

        // 不能超过1000
    }
}