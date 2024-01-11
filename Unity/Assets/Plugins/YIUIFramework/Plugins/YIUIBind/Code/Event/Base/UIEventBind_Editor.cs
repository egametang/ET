#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Logger = YIUIFramework.Logger;

namespace YIUIFramework
{
    public abstract partial class UIEventBind
    {
        [GUIColor(0, 1, 1)]
        [Button("响应点击 只能响应无参方法", 20)]
        [PropertyOrder(-100)]
        private void TestOnClick()
        {
            if (m_UIEvent == null)
            {
                Logger.LogError($"未选择事件");
            }

            try
            {
                if (m_UIEvent.IsTaskEvent)
                {
                    m_UIEvent.InvokeAsync().Coroutine();
                }
                else
                {
                    m_UIEvent.Invoke();
                }
            }
            catch (Exception e)
            {
                Logger.LogError(e);
                throw;
            }
        }

        private const string c_ErrorTips = "当前事件表中无符合参数条件的事件";

        private IEnumerable<string> GetEventNameKeys()
        {
            if (m_EventTable == null)
            {
                Logger.LogErrorContext(this, $"{name}  请检查未设置 事件表");
                return null;
            }

            var list = m_EventTable.GetFilterParamTypeEventName(GetFilterParamType, IsTaskEvent);

            for (var i = list.Count - 1; i >= 0; i--)
            {
                var eventName = list[i];
                if (string.IsNullOrEmpty(eventName))
                {
                    list.RemoveAt(i);
                }
            }

            if (list.Count <= 0)
            {
                list.Add(c_ErrorTips);
            }
            else
            {
                list.Add(""); //为了清空当前选择的事件用的 否则无法取消当前绑定的事件
            }

            return list;
        }

        private void OnEventNameSelected()
        {
            if (m_EventName == c_ErrorTips)
            {
                m_EventName = "";
                Logger.LogError($"{c_ErrorTips} 请创建 提示: {GetFilterParamType.GetAllParamTypeTips()}");
            }

            UnbindEvent();
            m_UIEvent = null;
        }

        /// <summary>
        /// 绑定
        /// </summary>
        private void BindEvent()
        {
            m_UIEvent?.AddBind(this);
        }

        /// <summary>
        /// 解绑
        /// </summary>
        private void UnbindEvent()
        {
            m_UIEvent?.RemoveBind(this);
        }

        /// <summary>
        /// 对应的需要处理相关关联绑定
        /// </summary>
        public virtual void RemoveBind(UIEventBase uiEvent)
        {
            if (uiEvent == m_UIEvent)
            {
                m_EventName = null;
                m_UIEvent   = null;
            }
            else
            {
                Logger.LogError($"移除与当前不一致 请检查 当前{m_UIEvent.EventName} 移除{uiEvent.EventName}");
            }
        }

        protected void OnDestroy()
        {
            UnbindEvent();
        }

        protected void OnValidate()
        {
            OnRefreshEvent();
        }
    }
}
#endif