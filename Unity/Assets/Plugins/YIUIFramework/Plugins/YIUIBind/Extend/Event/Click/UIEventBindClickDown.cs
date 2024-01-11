using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace YIUIFramework
{
    /// <summary>
    /// 点击事件绑定
    /// 与按钮无关
    /// 只要是任何可以被射线检测的物体都可以响应点击事件
    /// </summary>
    [LabelText("点击 按下<null>")]
    [AddComponentMenu("YIUIBind/Event/点击按下 【ClickDown】 UIEventBindClickDown")]
    public class UIEventBindClickDown: UIEventBind, IPointerDownHandler
    {
        [SerializeField]
        [LabelText("可选组件")]
        private Selectable m_Selectable;

        public void OnPointerDown(PointerEventData eventData)
        {
            if (m_Selectable != null && !m_Selectable.interactable)
            {
                return;
            }

            try
            {
                OnUIEvent(eventData);
            }
            catch (Exception e)
            {
                Logger.LogError(e);
                throw;
            }
        }

        protected override bool IsTaskEvent => false;

        [NonSerialized]
        private List<EUIEventParamType> m_BaseFilterParamType = new List<EUIEventParamType> { };

        protected override List<EUIEventParamType> GetFilterParamType => m_BaseFilterParamType;

        private void Awake()
        {
            m_Selectable ??= GetComponent<Selectable>();
        }

        protected virtual void OnUIEvent(PointerEventData eventData)
        {
            m_UIEvent?.Invoke();
        }
    }
}