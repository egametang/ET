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

        private const string UnityModelCodes = "Unity.Model.Codes";
        private const string UnityHotfixCodes = "Unity.Hotfix.Codes";
        private const string UnityModelViewCodes = "Unity.ModelView.Codes";
        private const string UnityHotfixViewCodes = "Unity.HotfixView.Codes";

        public static readonly string[] AllHotfix =
        {
            DotNetHotfix, UnityHotfix, UnityHotfixView, 
            UnityHotfixCodes, UnityHotfixViewCodes
        };

        public static readonly string[] AllModel =
        {
            DotNetModel, UnityModel, 
            UnityModelView, UnityModel, UnityModelCodes
        };

        public static readonly string[] AllModelHotfix =
        {
            DotNetModel, DotNetHotfix, 
            UnityModel, UnityHotfix, UnityModelView, UnityHotfixView, 
            UnityModelCodes, UnityModelViewCodes, UnityHotfixCodes, UnityHotfixViewCodes
        };
        
        public static readonly string[] All =
        {
            DotNetCore, DotNetModel, DotNetHotfix, 
            UnityCore, UnityModel, UnityHotfix, UnityModelView, UnityHotfixView, 
            UnityModelCodes, UnityModelViewCodes, UnityHotfixCodes, UnityHotfixViewCodes
        };
        
        
    }
}