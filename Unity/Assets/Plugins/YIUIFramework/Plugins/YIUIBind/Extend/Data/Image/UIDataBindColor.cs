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
    [LabelText("改变颜色")]
    [RequireComponent(typeof(Graphic))]
    [AddComponentMenu("YIUIBind/Data/颜色 【Color】 UIDataBindColor")]
    public class UIDataBindColor : UIDataBindSelectBase
    {
        protected override int Mask()
        {
            return 1 << (int)EUIBindDataType.Color;
        }

        protected override int SelectMax()
        {
            return 1;
        }

        [SerializeField]
        [ReadOnly]
        [Required("必须有此组件")]
        [LabelText("图")]
        private Graphic m_Graphic;

        protected override void OnRefreshData()
        {
            base.OnRefreshData();
            m_Graphic ??= GetComponent<Graphic>();
        }

        protected override void OnValueChanged()
        {
            var dataValue = GetFirstValue<Color>();

            if (m_Graphic != null)
            {
                m_Graphic.color = dataValue;
            }
        }
    }
}