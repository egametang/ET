#if UNITY_EDITOR
namespace YIUIFramework.Editor
{
    internal class UICreateRedDotKeyCode : BaseTemplate
    {
        public override string EventName => "红点系统 key 枚举自动生成";

        private         bool m_AutoRefresh = false;
        public override bool AutoRefresh => m_AutoRefresh;

        private         bool m_ShowTips = false;
        public override bool ShowTips => m_ShowTips;

        public UICreateRedDotKeyCode(out bool result, string authorName, UICreateRedDotKeyData codeData) :
            base(authorName)
        {
            var path     = codeData.ClassPath;
            var template = $"{UIStaticHelper.UITemplatePath}/UICreateRedDotKeyTemplate.txt";
            CreateVo = new CreateVo(template, path);

            m_AutoRefresh       = codeData.AutoRefresh;
            m_ShowTips          = codeData.ShowTips;
            ValueDic["Content"] = codeData.Content;

            result = CreateNewFile();
        }
    }
}
#endif