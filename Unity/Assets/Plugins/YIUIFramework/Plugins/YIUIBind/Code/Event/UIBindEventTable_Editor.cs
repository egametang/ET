#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace YIUIFramework
{
    public sealed partial class UIBindEventTable
    {
        [GUIColor(1, 1, 0)]
        [Button("自动检查", 30)]
        [PropertyOrder(-999)]
        [ShowIf("@UIOperationHelper.CommonShowIf()")]
        public void AutoCheck()
        {
            if (!UIOperationHelper.CheckUIOperation(this)) return;

            var dicKey = m_EventDic.Keys.ToList();
            foreach (var oldName in dicKey)
            {
                if (string.IsNullOrEmpty(oldName))
                {
                    continue;
                }

                var newName = oldName;

                if (!oldName.CheckFirstName(NameUtility.EventName))
                {
                    newName = $"{NameUtility.FirstName}{NameUtility.EventName}{oldName}";
                }

                newName = newName.ChangeToBigName(NameUtility.EventName);

                if (oldName != newName)
                {
                    var uiData = m_EventDic[oldName];
                    m_EventDic.Remove(oldName);
                    m_EventDic.Add(newName, uiData);
                }
            }

            OnValidate();
        }

        [HideLabel]
        private enum EUITaskEventType
        {
            [LabelText("同步事件")]
            Sync,

            [LabelText("异步事件")]
            Async,
        }

        [ShowInInspector]
        [BoxGroup("添加新事件")]
        [EnumToggleButtons]
        [HideLabel]
        [NonSerialized]
        [PropertyOrder(-100)]
        [ShowIf("@UIOperationHelper.CommonShowIf()")]
        private EUITaskEventType m_UITaskEventType;

        [ShowInInspector]
        [BoxGroup("添加新事件")]
        [HideReferenceObjectPicker]
        [HideLabel]
        [PropertyOrder(-99)]
        [Delayed]
        [ShowIf("@UIOperationHelper.CommonShowIf()")]
        private string m_AddUIEventName = "";

        [BoxGroup("添加新事件")]
        [PropertyOrder(-98)]
        [OdinSerialize]
        [ShowInInspector]
        [LabelText("需要添加的事件参数")]
        [ShowIf("@UIOperationHelper.CommonShowIf()")]
        public List<EUIEventParamType> AllEventParamType = new List<EUIEventParamType>();

        [GUIColor(0, 1, 0)]
        [BoxGroup("添加新事件")]
        [Button("添加")]
        [PropertyOrder(-98)]
        [ShowIf("@UIOperationHelper.CommonShowIf()")]
        private void AddNewUIEvent()
        {
            if (string.IsNullOrEmpty(m_AddUIEventName))
            {
                UnityTipsHelper.ShowError($"必须填写名称才可以添加");
                return;
            }

            if (m_EventDic.ContainsKey(m_AddUIEventName))
            {
                UnityTipsHelper.ShowError($"已存在同名数据  请修改 {m_AddUIEventName}");
                return;
            }

            UIEventBase uiEventBase;

            switch (m_UITaskEventType)
            {
                case EUITaskEventType.Sync:
                    uiEventBase = UIEventBaseHelper.CreatorUIEventBase(m_AddUIEventName, AllEventParamType);
                    break;
                case EUITaskEventType.Async:
                    uiEventBase = UITaskEventBaseHelper.CreatorUITaskEventBase(m_AddUIEventName, AllEventParamType);
                    break;
                default:
                    uiEventBase = UIEventBaseHelper.CreatorUIEventBase(m_AddUIEventName, AllEventParamType);
                    throw new ArgumentOutOfRangeException();
            }

            if (uiEventBase == null)
            {
                UnityTipsHelper.ShowError($"创建失败 {m_AddUIEventName}");
                return;
            }

            m_EventDic.Add(m_AddUIEventName, uiEventBase);

            AllEventParamType = new List<EUIEventParamType>();
            m_AddUIEventName  = "";
            AutoCheck();
        }

        private void RemoveCallBack(UIEventBase uiEvent)
        {
            uiEvent.OnRemoveVariableCallBack();
            m_EventDic.Remove(uiEvent.EventName);
        }

        private void OnRemoveUIEvent(UIEventBase uiEvent)
        {
            if (!m_EventDic.ContainsValue(uiEvent))
            {
                UnityTipsHelper.ShowError($"不存在无法移除 {uiEvent.EventName}  请检查是否有配置错误");
                return;
            }

            //如果已经有绑定了 需要提醒是否移除
            if (uiEvent.GetBindCount() >= 1)
            {
                var callBackTips = $"{uiEvent.EventName} 已绑定 {uiEvent.GetBindCount()}个目标\n移除会强制清楚所有绑定 请确认是否需要移除!!!";
                UnityTipsHelper.CallBack(callBackTips, () => { RemoveCallBack(uiEvent); });
                return;
            }

            RemoveCallBack(uiEvent);
        }

        public List<string> GetAllEventName()
        {
            var allName = new List<string>();
            foreach (var name in m_EventDic.Keys)
            {
                allName.Add(name);
            }

            return allName;
        }

        public List<string> GetFilterParamTypeEventName(List<EUIEventParamType> list, bool taskEvent)
        {
            var allName = new List<string>();
            foreach (var eventValue in m_EventDic.Values)
            {
                if (eventValue.IsTaskEvent == taskEvent && eventValue.AllEventParamType.ParamEquals(list))
                {
                    allName.Add(eventValue.EventName);
                }
            }

            return allName;
        }

        private void OnValidate()
        {
            if (m_EventDic == null)
            {
                return;
            }

            if (UIOperationHelper.IsPlaying())
            {
                return;
            }

            foreach (var data in m_EventDic)
            {
                var uiEventName = data.Key;
                var uiEvent     = data.Value;
                uiEvent.ClearBinds();
                uiEvent.ChangeName(uiEventName);
                uiEvent.OnRemoveAction = OnRemoveUIEvent;
            }

            InitEventTable();
        }
    }
}

#endif