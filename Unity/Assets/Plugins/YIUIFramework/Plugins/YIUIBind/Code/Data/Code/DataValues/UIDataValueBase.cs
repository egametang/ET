using System;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using Logger = YIUIFramework.Logger;

namespace YIUIFramework
{
    /// <summary>
    /// 数据基类
    /// 泛型 扩展请参考其他数据 注意满足需求
    /// 具体请查看文档
    /// @李胜扬
    /// </summary>
    [Serializable]
    public abstract class UIDataValueBase<T> : UIDataValue, IUIDataValue<T>
    {
        [OdinSerialize]
        [LabelText("值")]
        [HideReferenceObjectPicker]
        [Delayed]
        #if UNITY_EDITOR
        [OnValueChanged("OnValueChanged")]
        #endif
        private T m_Value;

        #if UNITY_EDITOR
        private void OnValueChanged()
        {
            InvokeValueChangAction();
        }
        #endif

        //基类中的事件 双参数 1 新值 2 老值
        private Action<T, T> m_OnValueChangeAction;

        public void AddValueChangeAction(Action<T, T> action)
        {
            m_OnValueChangeAction -= action;
            m_OnValueChangeAction += action;
        }

        public void RemoveValueChangeAction(Action<T, T> action)
        {
            m_OnValueChangeAction -= action;
        }

        public void InvokeValueChangAction(T newValue, T oldValue)
        {
            try
            {
                m_OnValueChangeAction?.Invoke(newValue, oldValue);
            }
            catch (Exception e)
            {
                Logger.LogError(e);
                throw;
            }
        }

        public T GetValue()
        {
            return m_Value;
        }

        internal sealed override bool SetValueFrom(UIDataValue dataValue)
        {
            return SetValueFrom(dataValue, true); //必须使用强制刷新
        }

        public bool SetValueFrom(UIDataValue dataValue, bool force, bool notify = true)
        {
            if (dataValue == null)
            {
                Logger.LogError($"{typeof(T)} 失败，Value为空");
                return false;
            }

            if (!(dataValue is UIDataValueBase<T>))
            {
                Logger.LogError($"失败，类型不一致 当前类型 {typeof(T)} 传入类型 {dataValue.UIDataValueType}");
                return false;
            }

            return SetValue(dataValue.GetValue<T>(), force, notify);
        }

        public bool SetValue(T value, bool force = false, bool notify = true)
        {
            if (!force && EqualsValue(value)) return false;

            var oldValue = m_Value;
            SetValueFrom(value);
            InvokeValueChangAction();
            if (notify)
                InvokeValueChangAction(value, oldValue);
            return true;
        }

        protected virtual void SetValueFrom(T value)
        {
            m_Value = value;
        }

        protected abstract bool EqualsValue(T value);
    }
}