#if UNITY_EDITOR

using System;
using Sirenix.OdinInspector;

namespace YIUIFramework.Editor
{
    [Flags]
    [LabelText("Test 测试")]
    public enum ETestMacroType : long
    {
        [LabelText("所有")]
        ALL = -1,

        [LabelText("无")]
        NONE = 0,

        [LabelText("测试1")]
        MACRO_TEST_1 = 1,

        [LabelText("测试2")]
        MACRO_TEST_2 = 1 << 1,

        [LabelText("测试3")]
        MACRO_TEST_3 = 1 << 2,

        [LabelText("测试4")]
        MACRO_TEST_4 = 1 << 3,

        [LabelText("测试5")]
        MACRO_TEST_5 = 1 << 4,

        [LabelText("测试6")]
        MACRO_TEST_6 = 1 << 5,
    }

    [Serializable]
    public class TestMacroData : MacroDataBase<ETestMacroType>
    {
        protected override void Init()
        {
            MacroEnumType = (ETestMacroType)MacroHelper.InitEnumValue<ETestMacroType>(UIMacroModule.BuildTargetGroup);
        }
    }
}
#endif