namespace ET.Analyzer
{
    public static class AnalyzeAssembly
    {
        public const string ServerModel = "Model";

        public const string SerVerHotfix = "Hotfix";

        public const string UnityModel = "Unity.Model";

        public const string UnityHotfix = "Unity.Hotfix";

        public const string UnityModelView = "Unity.ModelView";

        public const string UnityHotfixView = "Unity.HotfixView";

        public static readonly string[] AllHotfix = { SerVerHotfix, UnityHotfix, UnityHotfixView };

        public static readonly string[] AllModel = { ServerModel, UnityModel, UnityModelView };

        public static readonly string[] All = { ServerModel, SerVerHotfix, UnityModel, UnityHotfix, UnityModelView, UnityHotfixView };
    }
}