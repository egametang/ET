namespace ET.Server
{
    public static partial class LocationType
    {
        public const int Unit = PackageType.StateSync * 1000 + 1;
        public const int Player = PackageType.StateSync * 1000 + 2;
        public const int GateSession = PackageType.StateSync * 1000 + 3;
    }
}