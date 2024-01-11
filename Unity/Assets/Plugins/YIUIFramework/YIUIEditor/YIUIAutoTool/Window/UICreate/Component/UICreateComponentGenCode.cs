#if UNITY_EDITOR

namespace YIUIFramework.Editor
{
    public class UICreateComponentGenCode: BaseTemplate
    {
        private         string m_EventName = "ET-Component Gen 自动生成";
        public override string EventName => m_EventName;

        public override bool Cover => true;

        private         bool m_AutoRefresh = false;
        public override bool AutoRefresh => m_AutoRefresh;

        private         bool m_ShowTips = false;
        public override bool ShowTips => m_ShowTips;

        public UICreateComponentGenCode(out bool result, string authorName, UICreateBaseData codeData): base(authorName)
        {
            var path     = $"{UIStaticHelper.UIETComponentGenPath}/{codeData.PkgName}/{codeData.ResName}ComponentGen.cs";
            var template = $"{UIStaticHelper.UITemplatePath}/UICreateComponentGenTemplate.txt";
            CreateVo = new CreateVo(template, path);

            m_EventName               = $"{codeData.ResName} ET-Component Gen 自动生成";
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
            var panelLayer = "";
            if (codeData.CodeType == EUICodeType.Panel)
            {
                panelLayer = $", EPanelLayer.{codeData.PanelLayer}";
            }

            ValueDic["PanelLayer"] = panelLayer;

            result = CreateNewFile();
        }
    }
}
#endif