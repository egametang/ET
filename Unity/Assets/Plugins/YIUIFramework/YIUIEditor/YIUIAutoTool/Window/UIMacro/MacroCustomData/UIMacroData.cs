#if UNITY_EDITOR

using System;
using Sirenix.OdinInspector;

namespace YIUIFramework.Editor
{
    [Flags]
    [LabelText("YIUI")]
    public enum EYIUIMacroType : long
    {
        [LabelText("所有")]
        ALL = -1,

        [LabelText("无")]
        NONE = 0,

        [LabelText("模拟非编辑器状态")]
        YIUIMACRO_SIMULATE_NONEEDITOR = 1,

        [LabelText("UIBing 初始化")]
        YIUIMACRO_BIND_INITIALIZE = 1 << 1,

        [LabelText("界面开关")]
        YIUIMACRO_PANEL_OPENCLOSE = 1 << 2,

        [LabelText("运行时调试UI")]
        YIUIMACRO_BIND_RUNTIME_EDITOR = 1 << 3,

        [LabelText("红点堆栈收集")]
        YIUIMACRO_REDDOT_STACK = 1 << 4,
    }

    [Serializable]
    [MacroAttribute]
    public class UIMacroData : MacroDataBase<EYIUIMacroType>
    {
        protected override void Init()
        {
            MacroEnumType = (EYIUIMacroType)MacroHelper.InitEnumValue<EYIUIMacroType>(UIMacroModule.BuildTargetGroup);
        }
    }
}
#endif