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
    [LabelText("UI旋转")]
    [RequireComponent(typeof(RectTransform))]
    [AddComponentMenu("YIUIBind/Data/UI旋转 【RectRot1】 UIDataBindRectRot1 l")]
    public class UIDataBindRectRot1 : UIDataBindSelectBase
    {
        protected override int Mask()
        {
            return 1 << (int)EUIBindDataType.Float;
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
        [InfoBox("那个轴有值就是控制那个轴 单位1, 多的就是倍数 默认使用 Z轴旋转1")]
        [LabelText("目标轴")]
        private Vector3 m_TargetRot = Vector3.forward;

        protected override void OnRefreshData()
        {
            base.OnRefreshData();
            m_RectTransform ??= GetComponent<RectTransform>();
        }

        protected override void OnValueChanged()
        {
            if (m_RectTransform == null) return;

            var dataValue = GetFirstValue<float>();

            m_RectTransform.rotation = Quaternion.Euler(m_TargetRot * dataValue);
        }
    }
}