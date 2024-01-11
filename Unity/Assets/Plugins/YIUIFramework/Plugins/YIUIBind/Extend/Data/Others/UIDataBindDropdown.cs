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
    [LabelText("下拉菜单")]
    [RequireComponent(typeof(Dropdown))]
    [AddComponentMenu("YIUIBind/Data/下拉菜单 【Dropdown】 UIDataBindDropdown")]
    public class UIDataBindDropdown : UIDataBindSelectBase
    {
        protected override int Mask()
        {
            return 1 << (int)EUIBindDataType.Int;
        }

        protected override int SelectMax()
        {
            return 1;
        }

        [ReadOnly]
        [Required("必须有此组件")]
        [SerializeField]
        [LabelText("下拉菜单")]
        private Dropdown m_Dropdown;

        protected override void OnRefreshData()
        {
            base.OnRefreshData();
            m_Dropdown ??= GetComponent<Dropdown>();
            if (m_Dropdown != null)
            {
                m_Dropdown.onValueChanged.RemoveListener(OnDropdownValueChanged);
                m_Dropdown.onValueChanged.AddListener(OnDropdownValueChanged);
            }
        }

        private void OnDropdownValueChanged(int index)
        {
            SetFirstValue<int>(index);
        }

        protected override void OnValueChanged()
        {
            if (m_Dropdown == null) return;

            var dataValue = GetFirstValue<int>();

            if (m_Dropdown.value != dataValue)
            {
                m_Dropdown.value = dataValue;
            }
        }
    }
}