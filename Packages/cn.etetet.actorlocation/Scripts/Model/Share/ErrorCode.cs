namespace ET
{
    public static partial class ErrorCode
    {
        public const int ERR_NotFoundActor = ERR_WithException + PackageType.ActorLocation * 1000 + 1;
        public const int ERR_RpcFail = ERR_WithException + PackageType.ActorLocation * 1000 + 2;
        public const int ERR_MessageTimeout = ERR_WithException + PackageType.ActorLocation * 1000 + 3;
        public const int ERR_ActorLocationSenderTimeout2 = ERR_WithException + PackageType.ActorLocation * 1000 + 4;
        public const int ERR_ActorLocationSenderTimeout3 = ERR_WithException + PackageType.ActorLocation * 1000 + 5;
        public const int ERR_ActorLocationSenderTimeout4 = ERR_WithException + PackageType.ActorLocation * 1000 + 6;
    }
}