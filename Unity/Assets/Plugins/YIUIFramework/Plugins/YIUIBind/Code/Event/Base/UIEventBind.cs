using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using YIUIFramework;
using Logger = YIUIFramework.Logger;

namespace YIUIFramework
{
    [ExecuteInEditMode]
    public abstract partial class UIEventBind : SerializedMonoBehaviour
    {
        [OdinSerialize]
        [ReadOnly]
        [HideReferenceObjectPicker]
        [Required("必须选择")]
        [HideLabel]
        [PropertyOrder(-999)]
        private UIBindEventTable m_EventTable;

        public UIBindEventTable EventTable => m_EventTable;

        [OdinSerialize]
        [LabelText("事件名称")]
        #if UNITY_EDITOR
        [ValueDropdown("GetEventNameKeys")]
        [OnValueChanged("OnEventNameSelected")]
        [EnableIf("@UIOperationHelper.CommonShowIf()")]
        #endif
        [PropertyOrder(-99)]
        protected string m_EventName = null;
        
        /// <summary>
        /// 当前的UI事件
        /// </summary>
        [OdinSerialize]
        [HideInInspector]
        protected UIEventBase m_UIEvent;

        private UIEventBase GetEvent(string eventName)
        {
            if (string.IsNullOrEmpty(eventName))
            {
                //Logger.LogErrorContext(this,$"{name} 尝试获取一个空名称的事件 请检查");
                return null;
            }

            if (m_EventTable == null)
            {
                Logger.LogErrorContext(this, $"{name} 事件表==ull 请检查");
                return null;
            }

            var uiEvent = m_EventTable.FindEvent(eventName);
            if (uiEvent == null)
            {
                Logger.LogErrorContext(this, $"{name}没找到这个事件 {eventName} 请检查配置");
            }

            return uiEvent;
        }

        protected abstract bool IsTaskEvent { get; }

        protected abstract List<EUIEventParamType> GetFilterParamType { get; }

        private bool m_Binded;

        internal void Initialize(bool refresh = false)
        {
            if (!refresh && m_Binded) return;

            m_Binded = true;
            OnRefreshEvent();
        }

        private void RefreshEventName()
        {
            if (m_UIEvent != null)
            {
                m_EventName = m_UIEvent.EventName;
            }
        }

        protected virtual void RefreshBind()
        {
        }

        private void OnRefreshEvent()
        {
            RefreshEventTable();
            #if UNITY_EDITOR
            UnbindEvent();
            #endif
            RefreshEventName();
            m_UIEvent = GetEvent(m_EventName);
            RefreshEventName();
            #if UNITY_EDITOR
            BindEvent();
            #endif
            RefreshBind();
        }

        private void RefreshEventTable()
        {
            if (m_EventTable == null)
            {
                m_EventTable = this.GetComponentInParentHard<UIBindEventTable>();
            }
        }
    }
}