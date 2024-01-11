#if UNITY_EDITOR
using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace YIUIFramework.Editor
{
    public class CreateUIBindProviderCode : BaseTemplate
    {
        public override string EventName => "UI反射动态码";

        public override bool Cover => true;

        public override bool AutoRefresh => true;

        public override bool ShowTips => false;

        public CreateUIBindProviderCode(out bool result, string authorName, UIBindProviderData codeData) : base(
            authorName)
        {
            var path     = $"{UIStaticHelper.UIETComponentGenPath}/{codeData.Name}.cs";
            var template = $"{UIStaticHelper.UITemplatePath}/UIBindProviderTemplate.txt";
            CreateVo = new CreateVo(template, path);

            ValueDic["Count"]   = codeData.Count.ToString();
            ValueDic["Content"] = codeData.Content;

            result = CreateNewFile();
        }
    }
}
#endif