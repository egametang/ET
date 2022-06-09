namespace ET.Analyzer
{
    public static class AnalyzeAssembly
    {
        public const string AppsModel = "Apps.Model";

        public const string AppsHotfix = "Apps.Hotfix";

        public const string UnityModel = "Unity.Model";

        public const string UnityHotfix = "Unity.Hotfix";

        public const string UnityModelView = "Unity.ModelView";

        public const string UnityHotfixView = "Unity.HotfixView";

        public static readonly string[] AllHotfix = { AppsHotfix, UnityHotfix, UnityHotfixView };

        public static readonly string[] AllModel = { AppsModel, UnityModel, UnityModelView };

        public static readonly string[] All = { AppsModel, AppsHotfix, UnityModel, UnityHotfix, UnityModelView, UnityHotfixView };
    }
}