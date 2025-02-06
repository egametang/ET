using System.IO;

namespace ET
{
    public static class AnalyzeAssembly
    {
        private const string Core = "ET.Core";
        private const string Model = "ET.Model";
        private const string Hotfix = "ET.Hotfix";
        private const string ModelView = "ET.ModelView";
        private const string HotfixView = "ET.HotfixView";

        public static readonly string[] AllHotfix =
        [
            Hotfix, HotfixView
        ];

        public static readonly string[] AllModel =
        [
            Model, ModelView
        ];

        public static readonly string[] AllModelHotfix =
        [
            Model, Hotfix, ModelView, HotfixView
        ];
        
        public static readonly string[] All =
        [
            Core, Model, Hotfix, ModelView, HotfixView
        ];
        
        public static readonly string[] AllLogicModel =
        [
            Model
        ];
    }
    
}