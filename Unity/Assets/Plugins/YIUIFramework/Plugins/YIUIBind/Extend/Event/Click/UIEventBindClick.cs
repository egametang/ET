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
    [LabelText("点击<null>")]
    [AddComponentMenu("YIUIBind/Event/点击 【Click】 UIEventBindClick")]
    public class UIEventBindClick: UIEventBind, IPointerClickHandler
    {
        [SerializeField]
        [LabelText("拖拽时不响应点击")]
        private bool m_SkipWhenDrag;

        [SerializeField]
        [LabelText("可选组件")]
        private Selectable m_Selectable;

        public void OnPointerClick(PointerEventData eventData)
        {
            if (m_Selectable != null && !m_Selectable.interactable)
            {
                return;
            }

            if (m_SkipWhenDrag && eventData.dragging)
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