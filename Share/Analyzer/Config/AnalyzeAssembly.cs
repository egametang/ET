using System.IO;

namespace ET.Analyzer
{
    public static class AnalyzeAssembly
    {
        public const string DotNetCore = "Core";
        public const string DotNetModel = "Model";
        public const string DotNetHotfix = "Hotfix";

        public const string UnityCore = "Unity.Core";
        public const string UnityModel = "Unity.Model";
        public const string UnityHotfix = "Unity.Hotfix";
        public const string UnityModelView = "Unity.ModelView";
        public const string UnityHotfixView = "Unity.HotfixView";

        public static readonly string[] AllHotfix =
        {
            DotNetHotfix, UnityHotfix, UnityHotfixView,
        };

        public static readonly string[] AllModel =
        {
            DotNetModel, UnityModel, UnityModelView
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
        
        public static readonly string[] AllLogicModel =
        {
            DotNetModel, UnityModel
        };
    }
    
}