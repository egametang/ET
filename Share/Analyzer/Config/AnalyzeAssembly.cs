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

        public static readonly string[] AllHotfix = new string[] { SerVerHotfix, UnityHotfix, UnityHotfixView };

        public static readonly string[] AllModel = new string[] { ServerModel, UnityModel, UnityModelView };

        public static readonly string[] All = new string[] { ServerModel, SerVerHotfix, UnityModel, UnityHotfix, UnityModelView, UnityHotfixView };
    }
}