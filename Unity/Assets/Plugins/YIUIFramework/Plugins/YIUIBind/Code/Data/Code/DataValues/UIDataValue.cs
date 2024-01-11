using System;
using Sirenix.OdinInspector;
using Logger = YIUIFramework.Logger;

namespace YIUIFramework
{
    [Serializable]
    [HideLabel]
    public abstract class UIDataValue
    {
        [ShowInInspector]
        [PropertyOrder(-99)]
        public abstract EUIBindDataType UIBindDataType { get; }

        public abstract Type UIDataValueType { get; }

        /// <summary>
        /// 从另一个Value设置数据
        /// </summary>
        internal abstract bool SetValueFrom(UIDataValue dataValue);

        //值改变消息 无参
        private Action m_OnValueChangAction;

        internal void AddValueChangeAction(Action action)
        {
            m_OnValueChangAction -= action;
            m_OnValueChangAction += action;
        }

        internal void RemoveValueChangeAction(Action action)
        {
            m_OnValueChangAction -= action;
        }

        internal void InvokeValueChangAction()
        {
            try
            {
                m_OnValueChangAction?.Invoke();
            }
            catch (Exception e)
            {
                Logger.LogError(e);
                throw;
            }
        }
    }
}