namespace ET
{
    [UniqueId]
    public static partial class SceneType
    {
        public const int All = 0;
        public const int Main = PackageType.Core * 1000 + 1;
        public const int NetInner = PackageType.Core * 1000 + 2;
        public const int NetClient = PackageType.Core * 1000 + 3;
    }
}