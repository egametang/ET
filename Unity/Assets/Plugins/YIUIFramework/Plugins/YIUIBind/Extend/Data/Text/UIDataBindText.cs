using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using Logger = YIUIFramework.Logger;

namespace YIUIFramework
{
    [RequireComponent(typeof(Text))]
    [LabelText("Text 文本")]
    [AddComponentMenu("YIUIBind/Data/文本 【Text】 UIDataBindText")]
    public sealed class UIDataBindText : UIDataBindTextBase
    {
        [SerializeField]
        [ReadOnly]
        [Required("必须有此组件")]
        [LabelText("文本")]
        private Text m_Text;

        protected override void OnInit()
        {
            m_Text ??= GetComponent<Text>();
            if (m_Text == null)
            {
                Logger.LogError($"{name} 错误没有 Text 组件");
                return;
            }

            if (!m_ChangeEnabled && !m_Text.enabled)
            {
                Logger.LogError($"{name} 当前文本禁止修改Enabled 且当前处于隐藏状态 可能会出现问题 请检查");
            }
        }

        protected override void SetEnabled(bool value)
        {
            if (m_Text == null) return;
            m_Text.enabled = value;
        }

        protected override void SetText(string value)
        {
            m_Text.text = value;
        }

        protected override bool ExistText()
        {
            if (m_Text == null)
            {
                m_Text = GetComponent<Text>();
            }

            return m_Text != null;
        }
    }
}