using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace YIUIFramework
{
    [Serializable]
    [LabelText("滚动条")]
    [RequireComponent(typeof(Scrollbar))]
    [AddComponentMenu("YIUIBind/Data/滚动条 【Scrollbar】 UIDataBindScrollbar")]
    public class UIDataBindScrollbar : UIDataBindSelectBase
    {
        [ReadOnly]
        [Required("必须有此组件")]
        [SerializeField]
        [LabelText("滚动条")]
        private Scrollbar m_Scrollbar;

        [InfoBox(">0时 则设置值时会以动画的形式慢慢变化 需要运行时")]
        [SerializeField]
        [LabelText("滑动动画速度")]
        #if UNITY_EDITOR
        [OnValueChanged("OnTweenSpeedValueChanged")]
        #endif
        private float m_TweenSpeed = 0.0f;

        [SerializeField]
        private ETweenType m_TweenType = ETweenType.DoubleWay;

        private float m_TargetValue;
        private bool  m_PlayingTween = false;

        protected override int Mask()
        {
            return 1 << (int)EUIBindDataType.Float;
        }

        protected override int SelectMax()
        {
            return 1;
        }

        protected override void OnRefreshData()
        {
            base.OnRefreshData();
            m_Scrollbar ??= GetComponent<Scrollbar>();
            if (m_Scrollbar == null) return;

            var dataValue = GetFirstValue<float>();
            m_TargetValue     = dataValue;
            m_Scrollbar.value = dataValue;
            m_PlayingTween    = false;
        }

        protected override void OnValueChanged()
        {
            if (m_Scrollbar == null) return;

            var dataValue = GetFirstValue<float>();
            if (m_TweenSpeed > 0.0f && Application.isPlaying)
            {
                m_TargetValue  = dataValue;
                m_PlayingTween = true;
            }
            else
            {
                m_TargetValue     = dataValue;
                m_Scrollbar.value = dataValue;
                m_PlayingTween    = false;
            }
        }

        #if UNITY_EDITOR
        private void OnTweenSpeedValueChanged()
        {
            //如果突然吧动画速度改为0
            //且当前正在动画中 那么会错误 虽然只有editor才会出现
            //这里还是处理
            if (m_TweenSpeed <= 0)
            {
                if (!Mathf.Approximately(m_Scrollbar.value, m_TargetValue))
                {
                    m_Scrollbar.value = m_TargetValue;
                }
            }
        }
        #endif

        private void Update()
        {
            if (m_PlayingTween &&
                m_TweenSpeed > 0.0f &&
                !Mathf.Approximately(m_Scrollbar.value, m_TargetValue))
            {
                switch (m_TweenType)
                {
                    case ETweenType.IncreaseOnly:
                        UpdateIncreaseOnly();
                        break;
                    case ETweenType.DecreaseOnly:
                        UpdateDecreaseOnly();
                        break;
                    case ETweenType.DoubleWay:
                        UpdateDoubleWay();
                        break;
                }
            }
        }

        private void UpdateIncreaseOnly()
        {
            if (m_TargetValue > m_Scrollbar.value)
            {
                UpdateDoubleWay();
                return;
            }

            var movement = m_TweenSpeed * Time.deltaTime;
            var newValue = m_Scrollbar.value + movement;
            if (newValue >= 1)
            {
                m_Scrollbar.value = 0;
            }
            else
            {
                m_Scrollbar.value = newValue;
            }
        }

        private void UpdateDecreaseOnly()
        {
            if (m_TargetValue < m_Scrollbar.value)
            {
                UpdateDoubleWay();
                return;
            }

            var movement = m_TweenSpeed * Time.deltaTime;
            var newValue = m_Scrollbar.value - movement;
            if (newValue <= 0)
            {
                m_Scrollbar.value = 1;
            }
            else
            {
                m_Scrollbar.value = newValue;
            }
        }

        private void UpdateDoubleWay()
        {
            var offset   = m_TargetValue - m_Scrollbar.value;
            var movement = m_TweenSpeed * Time.deltaTime;
            if (movement > Mathf.Abs(offset))
            {
                m_Scrollbar.value = m_TargetValue;
                m_PlayingTween    = false;
            }
            else
            {
                if (offset > 0.0f)
                {
                    m_Scrollbar.value += movement;
                }
                else
                {
                    m_Scrollbar.value -= movement;
                }
            }
        }
    }
}