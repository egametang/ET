#if UNITY_EDITOR

namespace YIUIFramework.Editor
{
    public class UICreateComponentCode: BaseTemplate
    {
        private         string m_EventName = "ET-Component 代码创建";
        public override string EventName => m_EventName;

        public override bool Cover => false;

        private         bool m_AutoRefresh = false;
        public override bool AutoRefresh => m_AutoRefresh;

        private         bool m_ShowTips = false;
        public override bool ShowTips => m_ShowTips;

        public UICreateComponentCode(out bool result, string authorName, UICreateBaseData codeData): base(authorName)
        {
            var path     = $"{UIStaticHelper.UIETComponentPath}/{codeData.PkgName}/{codeData.ResName}Component.cs";
            var template = $"{UIStaticHelper.UITemplatePath}/UICreateComponentTemplate.txt";
            CreateVo = new CreateVo(template, path);

            m_EventName               = $"{codeData.ResName} ET-Component 自动生成";
            m_AutoRefresh             = codeData.AutoRefresh;
            m_ShowTips                = codeData.ShowTips;
            ValueDic["Namespace"]     = codeData.Namespace;
            ValueDic["PkgName"]       = codeData.PkgName;
            ValueDic["ResName"]       = codeData.ResName;
            ValueDic["Variables"]     = codeData.Variables;
            ValueDic["UIBind"]        = codeData.UIBind;
            ValueDic["UIUnBind"]      = codeData.UIUnBind;
            ValueDic["VirtualMethod"] = codeData.VirtualMethod;
            ValueDic["PanelViewEnum"] = codeData.PanelViewEnum;
            ValueDic["CodeType"]      = codeData.CodeType.ToString();
            
            result = CreateNewFile();
        }
    }
}
#endif