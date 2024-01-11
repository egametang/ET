using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace YIUIFramework
{
    [DetailedInfoBox("可选择的Toggle/Button ...类的",
        @"改变是否可触摸 Toggle / inputField / button / dropdown 这些都可以用 
在某些情况下不允许点击时  灰色点击那种 就可以用这个")]
    [RequireComponent(typeof(Toggle))]
    [LabelText("是否可选择")]
    [AddComponentMenu("YIUIBind/Data/开关 【Toggle】 UIDataBindSelectable")]
    public sealed class UIDataBindToggle : UIDataBindBool
    {
        [SerializeField]
        [ReadOnly]
        [Required("必须有此组件")]
        [LabelText("开关")]
        private Toggle m_Toggle;

        protected override void OnRefreshData()
        {
            base.OnRefreshData();
            m_Toggle ??= GetComponent<Toggle>();
        }

        protected override void OnValueChanged()
        {
            if (m_Toggle == null) return;

            m_Toggle.isOn = GetResult();
        }
    }
}