#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Sirenix.OdinInspector;

namespace YIUIFramework.Editor
{
    [HideLabel]
    [Serializable]
    [HideReferenceObjectPicker]
    public abstract class MacroDataBase
    {
        public void ResetMacro()
        {
            MacroHelper.RemoveMacro(GetAll(), UIMacroModule.BuildTargetGroup);
            MacroHelper.AddMacro(GetSelect(), UIMacroModule.BuildTargetGroup);
        }

        public abstract List<string> GetAll();
        public abstract List<string> GetSelect();
    }

    /// <summary>
    /// 宏数据基类
    /// </summary>
    [HideLabel]
    [HideReferenceObjectPicker]
    [Serializable]
    public abstract class MacroDataBase<T> : MacroDataBase where T : struct
    {
        [LabelText("宏 枚举")]
        [ShowInInspector]
        protected T MacroEnumType;

        protected MacroDataBase()
        {
            Init();
        }

        /// <summary>
        /// 初始化宏枚举值
        /// </summary>
        protected abstract void Init();

        /// <summary>
        /// 获取当前枚举所有宏
        /// </summary>
        /// <returns></returns>
        public override List<string> GetAll()
        {
            return MacroHelper.GetEnumAll<T>();
        }

        /// <summary>
        /// 获取选择宏
        /// </summary>
        /// <returns></returns>
        public override List<string> GetSelect()
        {
            return MacroHelper.GetEnumSelect(MacroEnumType);
        }
    }
}
#endif