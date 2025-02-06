namespace ET
{
    public static partial class ErrorCode
    {
        public const int ERR_KcpRouterConnectFail = ERR_WithException + PackageType.Router * 1000 + 1;
        public const int ERR_KcpRouterTimeout = ERR_WithException + PackageType.Router * 1000 + 2;
        public const int ERR_KcpRouterSame = ERR_WithException + PackageType.Router * 1000 + 3;
        public const int ERR_KcpRouterRouterSyncCountTooMuchTimes = ERR_WithException + PackageType.Router * 1000 + 4;
        public const int ERR_KcpRouterTooManyPackets = ERR_WithException + PackageType.Router * 1000 + 5;
        public const int ERR_KcpRouterSyncCountTooMuchTimes = ERR_WithException + PackageType.Router * 1000 + 6;
    }
}