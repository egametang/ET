#if UNITY_EDITOR

namespace YIUIFramework.Editor
{
    public class UICreateViewSystemCode : BaseTemplate
    {
        private         string m_EventName = "ET-System 代码创建";
        public override string EventName => m_EventName;

        public override bool Cover => false;

        private         bool m_AutoRefresh = false;
        public override bool AutoRefresh => m_AutoRefresh;

        private         bool m_ShowTips = false;
        public override bool ShowTips => m_ShowTips;

        public UICreateViewSystemCode(out bool result, string authorName, UICreateSystemData codeData) : base(authorName)
        {
            var path     = $"{UIStaticHelper.UIETSystemPath}/{codeData.PkgName}/{codeData.ResName}ComponentSystem.cs";
            var template = $"{UIStaticHelper.UITemplatePath}/UICreateViewSystemTemplate.txt";
            CreateVo = new CreateVo(template, path);

            m_EventName               = $"{codeData.ResName} ET-System 自动生成";
            m_AutoRefresh             = codeData.AutoRefresh;
            m_ShowTips                = codeData.ShowTips;
            ValueDic["Namespace"]     = codeData.Namespace;
            ValueDic["PkgName"]       = codeData.PkgName;
            ValueDic["ResName"]       = codeData.ResName;
            
            if (!TemplateEngine.FileExists(CreateVo.SavePath))
            {
                result = CreateNewFile();
            }

            if (codeData.OverrideDic == null)
            {
                result = true;
                return;
            }

            result = OverrideCheckCodeFile(codeData.OverrideDic);
        }
    }
}
#endif