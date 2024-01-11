#if UNITY_EDITOR

namespace YIUIFramework.Editor
{
    public class UICreateSystemGenCode: BaseTemplate
    {
        private         string m_EventName = "ET-System Gen 自动生成";
        public override string EventName => m_EventName;

        public override bool Cover => true;

        private         bool m_AutoRefresh = false;
        public override bool AutoRefresh => m_AutoRefresh;

        private         bool m_ShowTips = false;
        public override bool ShowTips => m_ShowTips;

        public UICreateSystemGenCode(out bool result, string authorName, UICreateBaseData codeData): base(authorName)
        {
            var path     = $"{UIStaticHelper.UIETSystemGenPath}/{codeData.PkgName}/{codeData.ResName}ComponentSystemGen.cs";
            var template = $"{UIStaticHelper.UITemplatePath}/UICreateSystemGenTemplate.txt";
            CreateVo = new CreateVo(template, path);

            m_EventName               = $"{codeData.ResName} ET-System Gen 自动生成";
            m_AutoRefresh             = codeData.AutoRefresh;
            m_ShowTips                = codeData.ShowTips;
            ValueDic["Namespace"]     = codeData.Namespace;
            ValueDic["PkgName"]       = codeData.PkgName;
            ValueDic["ResName"]       = codeData.ResName;
            ValueDic["Variables"]     = codeData.Variables;
            ValueDic["UIFriendOf"]    = codeData.UIFriend;
            ValueDic["UIBase"]        = codeData.UIBase;
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