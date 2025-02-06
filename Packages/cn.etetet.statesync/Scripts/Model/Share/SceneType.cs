namespace ET
{
    public static partial class SceneType
    {
        public const int Http = PackageType.StateSync * 1000 + 1;
        public const int Map = PackageType.StateSync * 1000 + 2;
        public const int Robot = PackageType.StateSync * 1000 + 3;

        // 客户端
        public const int StateSync = PackageType.StateSync * 1000 + 20;
        public const int Current = PackageType.StateSync * 1000 + 21;
        public const int StateSyncView = PackageType.StateSync * 1000 + 24;
    }
}