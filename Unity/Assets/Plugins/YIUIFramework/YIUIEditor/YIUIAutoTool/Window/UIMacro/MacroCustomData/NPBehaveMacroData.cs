#if UNITY_EDITOR

using System;
using Sirenix.OdinInspector;

namespace YIUIFramework.Editor
{
    /// <summary>
    /// 行为树相关开启宏
    /// </summary>
    [Flags]
    [LabelText("NPBehave 行为树")]
    public enum ENPBheaveMacroType : long
    {
        [LabelText("所有")]
        ALL = -1,

        [LabelText("无")]
        NONE = 0,

        [LabelText("测试1")]
        MACRO_NPBEHAVE_TEST_1 = 1,

        [LabelText("测试2")]
        MACRO_NPBEHAVE_TEST_2 = 1 << 1,

        [LabelText("测试3")]
        MACRO_NPBEHAVE_TEST_3 = 1 << 2,

        [LabelText("测试4")]
        MACRO_NPBEHAVE_TEST_4 = 1 << 3,

        [LabelText("测试5")]
        MACRO_NPBEHAVE_TEST_5 = 1 << 4,

        [LabelText("测试6")]
        MACRO_NPBEHAVE_TEST_6 = 1 << 5,
    }

    /// <summary>
    /// NPBehave 相关宏
    /// </summary>
    [Serializable]
    [MacroAttribute]
    public class NPBehaveMacroData : MacroDataBase<ENPBheaveMacroType>
    {
        protected override void Init()
        {
            MacroEnumType =
                (ENPBheaveMacroType)MacroHelper.InitEnumValue<ENPBheaveMacroType>(UIMacroModule.BuildTargetGroup);
        }
    }
}
#endif