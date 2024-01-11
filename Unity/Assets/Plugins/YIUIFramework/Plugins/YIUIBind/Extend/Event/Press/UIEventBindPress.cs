using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace YIUIFramework
{
    /// <summary>
    /// 长按
    /// </summary>
    [LabelText("长按<obj>")]
    [AddComponentMenu("YIUIBind/Event/长按 【Press】 UIEventBindPress")]
    public class UIEventBindPress: UIEventBind, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        [SerializeField]
        [LabelText("长按时间")]
        private float m_PressTime;

        [SerializeField]
        [LabelText("按钮范围内长按才有效")]
        private bool m_SkipExit;

        [SerializeField]
        [LabelText("有效范围")]
        private Vector2 m_EffectiveRange = new Vector2(50, 50);

        [SerializeField]
        [LabelText("可选组件")]
        private Selectable m_Selectable;

        protected override bool IsTaskEvent => false;
        
        [NonSerialized]
        private List<EUIEventParamType> m_FilterParamType = new List<EUIEventParamType> { };

        protected override List<EUIEventParamType> GetFilterParamType => m_FilterParamType;

        private void Awake()
        {
            m_Selectable ??= GetComponent<Selectable>();
        }

        protected virtual void OnUIEvent()
        {
            m_UIEvent?.Invoke();
        }

        private bool    m_PointerExit;
        private Vector2 m_LastPos;

        public void OnPointerDown(PointerEventData eventData)
        {
            m_PointerExit = false;
            m_LastPos     = Input.mousePosition;
            if (m_PressTime <= 0)
            {
                PressEnd(0, 0, 0);
                return;
            }

            CountDownMgr.Inst?.Add(PressEnd, m_PressTime);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            CountDownMgr.Inst?.Remove(PressEnd);
        }

        private void PressEnd(double residuetime, double elapsetime, double totaltime)
        {
            if (m_Selectable != null && !m_Selectable.interactable) return;
            if (this.m_SkipExit && this.m_PointerExit) return;

            var inputX = Input.mousePosition.x;
            var inputY = Input.mousePosition.y;
            if (Mathf.Abs(m_LastPos.x - inputX) > m_EffectiveRange.x ||
                Mathf.Abs(m_LastPos.y - inputY) > m_EffectiveRange.y) return;

            try
            {
                OnUIEvent();
            }
            catch (Exception e)
            {
                Logger.LogError(e);
                throw;
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            m_PointerExit = true;
        }
    }
}