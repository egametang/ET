namespace ET.Analyzer
{
    public static class AnalyzeAssembly
    {
        public const string ServerModel = "Server.Model";

        public const string SerVerHotfix = "Server.Hotfix";

        public const string UnityModel = "Unity.Model";

        public const string UnityHotfix = "Unity.Hotfix";

        public const string UnityModelView = "Unity.ModelView";

        public const string UnityHotfixView = "Unity.HotfixView";

        public const string RobotModel = "Robot.Model";

        public const string RobotHotfix = "Robot.Hotfix";

        public static readonly string[] AllHotfix = new string[] {SerVerHotfix,UnityHotfix,UnityHotfixView,RobotHotfix};

        public static readonly string[] AllModel = new string[] { ServerModel, UnityModel, UnityModelView, RobotModel };

        public static readonly string[] All = new string[]
        {
            ServerModel, SerVerHotfix, UnityModel, UnityHotfix,
            UnityModelView, UnityHotfixView, RobotModel, RobotHotfix
        };


    }
}