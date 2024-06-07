namespace ET
{
    public static partial class CoroutineLockType
    {
        public const int Location = PackageType.ActorLocation * 1000 + 1;                  // location进程上使用
        public const int MessageLocationSender = PackageType.ActorLocation * 1000 + 2;       // MessageLocationSender中队列消息 
    }
}