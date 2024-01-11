using System;
using UnityEngine;

#if UNITY_EDITOR

namespace YIUIFramework.Editor
{
    public class UICreateETScriptComponentCode: BaseTemplate
    {
        private         string m_EventName = "ET-Component 代码创建";
        public override string EventName => m_EventName;

        public override bool Cover => false;

        private         bool m_AutoRefresh = false;
        public override bool AutoRefresh => m_AutoRefresh;

        private         bool m_ShowTips = false;
        public override bool ShowTips => m_ShowTips;

        public UICreateETScriptComponentCode(out bool result, string authorName, UICreateETScriptData codeData): base(authorName)
        {
            var path     = $"{codeData.ComponentPath}/{codeData.Name}Component.cs";
            var template = $"{UIStaticHelper.UITemplatePath}/ETScript/UICreateETScriptCommonTemplate.txt";
            CreateVo = new CreateVo(template, path);

            m_EventName           = $"{codeData.Name} ET-Component 自动生成";
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
            var life = "";
            foreach (EETLifeTpye lifeEnum in Enum.GetValues(typeof(EETLifeTpye)))
            {
                if (codeData.LifeTpye.HasFlag(lifeEnum))
                {
                    life += SwitchLife(lifeEnum);
                }
            }
            return life;
        }
        
        private string SwitchLife(EETLifeTpye life)
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
                    return ", IAwake";
                case EETLifeTpye.IUpdate:
                    return ", IUpdate";
                case EETLifeTpye.IDestroy:
                    return ", IDestroy";
                default:
                    Debug.LogError($"是否新增了类型 请检查 {life}");
                    break;
            }

            return "";
        }
        
    }
}
#endif