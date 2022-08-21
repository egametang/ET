namespace ET.Analyzer
{
    public static class AnalyzeAssembly
    {
        public const string AppsCore = "Core";
        
        public const string AppsModel = "Model";

        public const string AppsHotfix = "Hotfix";

        public const string UnityCore = "Unity.Core";

        public const string UnityModel = "Unity.Model";

        public const string UnityHotfix = "Unity.Hotfix";

        public const string UnityModelView = "Unity.ModelView";

        public const string UnityHotfixView = "Unity.HotfixView";

        public static readonly string[] AllHotfix = { AppsHotfix, UnityHotfix, UnityHotfixView };

        public static readonly string[] AllModel = { AppsModel, UnityModel, UnityModelView };

        public static readonly string[] AllModelHotfix = { AppsModel, AppsHotfix, UnityModel, UnityHotfix, UnityModelView, UnityHotfixView };
        
        public static readonly string[] All = { AppsCore, AppsModel, AppsHotfix, UnityCore, UnityModel, UnityHotfix, UnityModelView, UnityHotfixView };
        
        
    }
}