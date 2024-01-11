#if UNITY_EDITOR

using System;
using Sirenix.OdinInspector;

namespace YIUIFramework.Editor
{
    [Flags]
    [LabelText("Slate 时间线")]
    public enum ESlateMacroType : long
    {
        [LabelText("所有")]
        ALL = -1,

        [LabelText("无")]
        NONE = 0,

        [LabelText("测试1")]
        MACRO_SLATE_TEST_1 = 1,

        [LabelText("测试2")]
        MACRO_SLATE_TEST_2 = 1 << 1,

        [LabelText("测试3")]
        MACRO_SLATE_TEST_3 = 1 << 2,

        [LabelText("测试4")]
        MACRO_SLATE_TEST_4 = 1 << 3,

        [LabelText("测试5")]
        MACRO_SLATE_TEST_5 = 1 << 4,

        [LabelText("测试6")]
        MACRO_SLATE_TEST_6 = 1 << 5,
    }

    [Serializable]
    [MacroAttribute]
    public class SlateMacroData : MacroDataBase<ESlateMacroType>
    {
        protected override void Init()
        {
            MacroEnumType = (ESlateMacroType)MacroHelper.InitEnumValue<ESlateMacroType>(UIMacroModule.BuildTargetGroup);
        }
    }
}
#endif