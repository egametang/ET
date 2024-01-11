#if UNITY_EDITOR

using System;
using Sirenix.OdinInspector;

namespace YIUIFramework.Editor
{
    [Flags]
    [LabelText("Formula 公式")]
    public enum EFormulaMacroType : long
    {
        [LabelText("所有")]
        ALL = -1,

        [LabelText("无")]
        NONE = 0,

        [LabelText("公式最终结果")]
        MACRO_FORMULA_FUNCRESULT = 1,

        [LabelText("测试2")]
        MACRO_FORMULA_TEST_2 = 1 << 1,

        [LabelText("测试3")]
        MACRO_FORMULA_TEST_3 = 1 << 2,

        [LabelText("测试4")]
        MACRO_FORMULA_TEST_4 = 1 << 3,

        [LabelText("测试5")]
        MACRO_FORMULA_TEST_5 = 1 << 4,

        [LabelText("测试6")]
        MACRO_FORMULA_TEST_6 = 1 << 5,
    }

    /// <summary>
    /// Formula 公式 相关宏
    /// </summary>
    [Serializable]
    [MacroAttribute]
    public class FormulaMacroData : MacroDataBase<EFormulaMacroType>
    {
        protected override void Init()
        {
            MacroEnumType =
                (EFormulaMacroType)MacroHelper.InitEnumValue<EFormulaMacroType>(UIMacroModule.BuildTargetGroup);
        }
    }
}
#endif