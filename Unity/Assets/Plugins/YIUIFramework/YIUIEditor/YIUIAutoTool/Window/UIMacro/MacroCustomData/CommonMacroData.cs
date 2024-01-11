#if UNITY_EDITOR

using System;
using Sirenix.OdinInspector;

namespace YIUIFramework.Editor
{
    [Flags]
    [LabelText("通用")]
    public enum ECommonMacroType : long
    {
        [LabelText("所有")]
        ALL = -1,

        [LabelText("无")]
        NONE = 0,

        [LabelText("随机数 日志")]
        MACRO_RANDOM = 1,
        
    }

    [Serializable]
    [MacroAttribute]
    public class CommonMacroData : MacroDataBase<ECommonMacroType>
    {
        protected override void Init()
        {
            MacroEnumType =
                (ECommonMacroType)MacroHelper.InitEnumValue<ECommonMacroType>(UIMacroModule.BuildTargetGroup);
        }
    }
}
#endif