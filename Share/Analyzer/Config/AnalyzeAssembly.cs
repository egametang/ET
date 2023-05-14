namespace ET.Analyzer
{
    public static class AnalyzeAssembly
    {
        private const string DotNetCore = "Core";
        private const string DotNetModel = "Model";
        private const string DotNetHotfix = "Hotfix";

        private const string UnityCore = "Unity.Core";
        private const string UnityModel = "Unity.Model";
        private const string UnityHotfix = "Unity.Hotfix";
        private const string UnityModelView = "Unity.ModelView";
        private const string UnityHotfixView = "Unity.HotfixView";

        public static readonly string[] AllHotfix =
        {
            DotNetHotfix, UnityHotfix, UnityHotfixView, 
        };

        public static readonly string[] AllModel =
        {
            DotNetModel, UnityModel, 
            UnityModelView
        };

        public static readonly string[] AllModelHotfix =
        {
            DotNetModel, DotNetHotfix, 
            UnityModel, UnityHotfix, UnityModelView, UnityHotfixView, 
        };
        
        public static readonly string[] All =
        {
            DotNetCore, DotNetModel, DotNetHotfix, 
            UnityCore, UnityModel, UnityHotfix, UnityModelView, UnityHotfixView, 
        };

        public static readonly string[] ServerModelHotfix =
        {
            DotNetModel,DotNetHotfix,
        };

    }
}