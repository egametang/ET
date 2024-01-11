#if UNITY_EDITOR

using System;
using Sirenix.OdinInspector;

namespace YIUIFramework.Editor
{
    [Flags]
    [LabelText("技能通用")]
    public enum ESkillCommonMacroType : long
    {
        [LabelText("所有")]
        ALL = -1,

        [LabelText("无")]
        NONE = 0,

        [LabelText("技能 施法,加载")]
        MACRO_SKILL_COMMON_RELEASE = 1,

        [LabelText("屏蔽普通攻击")]
        MACRO_SKILL_COMMON_BAN_NORMALSKILL = 1 << 1,

        [LabelText("屏蔽法术攻击")]
        MACRO_SKILL_COMMON_BAN_SKILL = 1 << 2,

        [LabelText("技能事件")]
        MACRO_SKILL_COMMON_SKILL_EVENT = 1 << 3,

        [LabelText("测试5")]
        MACRO_SKILL_COMMON_TEST_5 = 1 << 4,

        [LabelText("测试6")]
        MACRO_SKILL_COMMON_TEST_6 = 1 << 5,
    }

    [Serializable]
    [MacroAttribute]
    public class SkillCommonMacroData : MacroDataBase<ESkillCommonMacroType>
    {
        protected override void Init()
        {
            MacroEnumType =
                (ESkillCommonMacroType)MacroHelper.InitEnumValue<ESkillCommonMacroType>(UIMacroModule.BuildTargetGroup);
        }
    }
}
#endif