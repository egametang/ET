using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace YIUIFramework
{
    [DetailedInfoBox("可选择的Toggle/Button ...类的",
        @"改变是否可触摸 Toggle / inputField / button / dropdown 这些都可以用 
在某些情况下不允许点击时  灰色点击那种 就可以用这个 因为他们都继承Selectable")]
    [RequireComponent(typeof(Selectable))]
    [LabelText("是否可选择")]
    [AddComponentMenu("YIUIBind/Data/交互 【Selectable】 UIDataBindSelectable")]
    public sealed class UIDataBindSelectable : UIDataBindBool
    {
        [SerializeField]
        [ReadOnly]
        [Required("必须有此组件")]
        [LabelText("可选组件")]
        private Selectable m_Selectable;

        protected override void OnRefreshData()
        {
            base.OnRefreshData();
            m_Selectable ??= GetComponent<Selectable>();
        }

        protected override void OnValueChanged()
        {
            if (m_Selectable == null) return;

            m_Selectable.interactable = GetResult();
        }
    }
}