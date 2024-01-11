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
    [LabelText("UI缩放")]
    [RequireComponent(typeof(RectTransform))]
    [AddComponentMenu("YIUIBind/Data/UI缩放 【RectScale3】 UIDataBindRectScale3")]
    public class UIDataBindRectScale3 : UIDataBindSelectBase
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

        protected override void OnRefreshData()
        {
            base.OnRefreshData();
            m_RectTransform ??= GetComponent<RectTransform>();
        }

        protected override void OnValueChanged()
        {
            if (m_RectTransform == null) return;

            var dataValue = GetFirstValue<Vector3>();

            m_RectTransform.localScale = dataValue;
        }
    }
}