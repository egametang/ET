#if UNITY_EDITOR

using System;
using Sirenix.OdinInspector;

namespace YIUIFramework.Editor
{
    [Flags]
    [LabelText("Buff")]
    public enum EBuffMacroType : long
    {
        [LabelText("所有")]
        ALL = -1,

        [LabelText("无")]
        NONE = 0,

        [LabelText("效果触发")]
        MACRO_BUFF_EFFECT = 1,

        [LabelText("效果触发 额外帧号")]
        MACRO_BUFF_EFFECT_FRAME = 1 << 1,

        [LabelText("效果 结果反馈")]
        MACRO_BUFF_EFFECT_TYPE_RESULT = 1 << 2,

        [LabelText("添加与移除时")]
        MACRO_BUFF_ADD_REMOVE = 1 << 3,

        [LabelText("添加与移除时帧号")]
        MACRO_BUFF_ADD_REMOVE_FRAME = 1 << 4,

        [LabelText("测试6")]
        MACRO_BUFF_TEST_6 = 1 << 5,
    }

    /// <summary>
    /// Buff 相关宏
    /// </summary>
    [Serializable]
    [MacroAttribute]
    public class BuffMacroData : MacroDataBase<EBuffMacroType>
    {
        protected override void Init()
        {
            MacroEnumType = (EBuffMacroType)MacroHelper.InitEnumValue<EBuffMacroType>(UIMacroModule.BuildTargetGroup);
        }
    }
}
#endif