namespace ET.Server
{
    public static partial class LocationType
    {
        public const int Unit = PackageType.Login * 1000 + 1;
        public const int Player = PackageType.Login * 1000 + 2;
        public const int GateSession = PackageType.Login * 1000 + 3;
    }
}