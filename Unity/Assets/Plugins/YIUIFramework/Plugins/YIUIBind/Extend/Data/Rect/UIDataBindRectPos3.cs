using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace YIUIFramework
{
    [Serializable]
    [LabelText("UI位置")]
    [RequireComponent(typeof(RectTransform))]
    [AddComponentMenu("YIUIBind/Data/UI位置 【RectPos3】 UIDataBindRectPos3 y")]
    public class UIDataBindRectPos3 : UIDataBindSelectBase
    {
        protected override int Mask()
        {
            return 1 << (int)EUIBindDataType.Vector3;
        }

        protected override int SelectMax()
        {
            return 1;
        }

        [SerializeField]
        [ReadOnly]
        [Required("必须有此组件")]
        [LabelText("UI变换组件")]
        private RectTransform m_RectTransform;

        [SerializeField]
        [LabelText("使用局部坐标否则世界坐标")]
        private bool m_LocalPosition = true;

        protected override void OnRefreshData()
        {
            base.OnRefreshData();
            m_RectTransform ??= GetComponent<RectTransform>();
        }

        protected override void OnValueChanged()
        {
            if (m_RectTransform == null) return;

            var dataValue = GetFirstValue<Vector3>();

            if (m_LocalPosition)
            {
                m_RectTransform.localPosition = dataValue;
            }
            else
            {
                m_RectTransform.position = dataValue;
            }
        }
    }
}