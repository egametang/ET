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
        
        public static readonly string[] AllLogicModel =
        {
            DotNetModel, UnityModel,UnityAllModel
        };
    }

    public static class UnityCodesPath
    {
        public static readonly string UnityModel = @"Unity\Assets\Scripts\Model\".Replace('\\',Path.DirectorySeparatorChar);
        public static readonly string UnityModelView = @"Unity\Assets\Scripts\ModelView\".Replace('\\',Path.DirectorySeparatorChar);
        public static readonly string UnityHotfix = @"Unity\Assets\Scripts\Hotfix\".Replace('\\',Path.DirectorySeparatorChar);
        public static readonly string UnityHotfixView = @"Unity\Assets\Scripts\HotfixView\".Replace('\\',Path.DirectorySeparatorChar);

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