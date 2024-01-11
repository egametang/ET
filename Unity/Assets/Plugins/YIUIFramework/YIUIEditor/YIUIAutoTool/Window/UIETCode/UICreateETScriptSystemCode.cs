using System;
using UnityEngine;

#if UNITY_EDITOR

namespace YIUIFramework.Editor
{
    public class UICreateETScriptSystemCode: BaseTemplate
    {
        private         string m_EventName = "ET-System 代码创建";
        public override string EventName => m_EventName;

        public override bool Cover => false;

        private         bool m_AutoRefresh = false;
        public override bool AutoRefresh => m_AutoRefresh;

        private         bool m_ShowTips = false;
        public override bool ShowTips => m_ShowTips;

        public UICreateETScriptSystemCode(out bool result, string authorName, UICreateETScriptData codeData): base(authorName)
        {
            var path     = $"{codeData.SystemPath}/{codeData.Name}ComponentSystem.cs";
            var template = $"{UIStaticHelper.UITemplatePath}/ETScript/UICreateETScriptSystemTemplate.txt";
            CreateVo = new CreateVo(template, path);

            m_EventName           = $"{codeData.Name} ET-System 自动生成";
            m_AutoRefresh         = codeData.AutoRefresh;
            m_ShowTips            = codeData.ShowTips;
            ValueDic["Namespace"] = codeData.Namespace;
            ValueDic["Name"]      = codeData.Name;
            ValueDic["Desc"]      = codeData.Desc;
            ValueDic["Life"]      = GetLife(codeData);

            result = CreateNewFile();
        }

        private string GetLife(UICreateETScriptData codeData)
        {
            var sb = SbPool.Get();
            foreach (EETLifeTpye lifeEnum in Enum.GetValues(typeof (EETLifeTpye)))
            {
                if (codeData.LifeTpye.HasFlag(lifeEnum))
                {
                    var content = SwitchLife(codeData, lifeEnum);
                    if (!string.IsNullOrEmpty(content))
                    {
                        sb.Append(content);
                        sb.AppendLine();
                    }
                }
            }

            return SbPool.PutAndToStr(sb);
        }

        private string SwitchLife(UICreateETScriptData codeData, EETLifeTpye life)
        {
            switch (life)
            {
                case EETLifeTpye.All:
                    break;
                case EETLifeTpye.Def:
                    break;
                case EETLifeTpye.None:
                    break;
                case EETLifeTpye.IAwake:
                    return string.Format(lifeTemp, codeData.Name, "Awake");
                case EETLifeTpye.IUpdate:
                    return string.Format(lifeTemp, codeData.Name, "Update");
                case EETLifeTpye.IDestroy:
                    return string.Format(lifeTemp, codeData.Name, "Destroy");
                default:
                    Debug.LogError($"是否新增了类型 请检查 {life}");
                    break;
            }

            return "";
        }

        private const string lifeTemp = @"
        [EntitySystem]
        private static void {1}(this {0}Component self)
        {{
        }}";
    }
}
#endif