using System.IO;

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

        public const string UnityCodes = "Unity.Codes";
        public const string UnityAllModel = "Unity.AllModel";
        public const string UnityAllHotfix = "Unity.AllHotfix";

        public static readonly string[] AllHotfix =
        {
            DotNetHotfix, UnityHotfix, UnityHotfixView,
            UnityAllHotfix,
        };

        public static readonly string[] AllModel =
        {
            DotNetModel, UnityModel, 
            UnityModelView,UnityAllModel
        };

        public static readonly string[] AllModelHotfix =
        {
            DotNetModel, DotNetHotfix, 
            UnityModel, UnityHotfix, UnityModelView, UnityHotfixView, 
            UnityAllModel, UnityAllHotfix,
        };
        
        public static readonly string[] All =
        {
            DotNetCore, DotNetModel, DotNetHotfix, 
            UnityCore, UnityModel, UnityHotfix, UnityModelView, UnityHotfixView, 
            UnityCodes,UnityAllModel, UnityAllHotfix,
        };

        public static readonly string[] ServerModelHotfix =
        {
            DotNetModel,DotNetHotfix,
        };
    }

    public static class UnityCodesPath
    {
        private static readonly string UnityModel = @"Unity\Assets\Scripts\Model\".Replace('\\',Path.DirectorySeparatorChar);
        private static readonly string UnityModelView = @"Unity\Assets\Scripts\ModelView\".Replace('\\',Path.DirectorySeparatorChar);
        private static readonly string UnityHotfix = @"Unity\Assets\Scripts\Hotfix\".Replace('\\',Path.DirectorySeparatorChar);
        private static readonly string UnityHotfixView = @"Unity\Assets\Scripts\HotfixView\".Replace('\\',Path.DirectorySeparatorChar);

        public static readonly string[] AllModelHotfix =
        {
            UnityModel, UnityHotfix, UnityModelView, UnityHotfixView, 
        };
        
        public static readonly string[] AllHotfix =
        {
            UnityHotfix, UnityHotfixView, 
        };

        public static readonly string[] AllModel =
        {
            UnityModel, UnityModelView
        };
    }
}