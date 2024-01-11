using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;

namespace YIUIFramework
{
    [LabelText("任意Component的显隐")]
    [AddComponentMenu("YIUIBind/Data/显隐 【ActiveComponent】 UIDataBindActiveComponent l")]
    public sealed class UIDataBindActiveComponent : UIDataBindBool
    {
        [SerializeField]
        [LabelText("控制的目标")]
        [Required("必须有此组件")]
        private Behaviour m_Target;

        [SerializeField]
        [LabelText("过度类型")]
        private UITransitionModeEnum m_TransitionMode = UITransitionModeEnum.Instant;

        [SerializeField]
        [LabelText("过度时间")]
        [ShowIf("m_TransitionMode", UITransitionModeEnum.Fade)]
        private float m_TransitionTime = 0.1f;

        protected override void OnValueChanged()
        {
            if (m_Target == null)
                return;

            var result = GetResult();

            if (m_TransitionMode == UITransitionModeEnum.Instant)
            {
                m_Target.enabled = result;
            }
            else
            {
                m_WaitForSeconds ??= new WaitForSeconds(m_TransitionTime);
                m_Coroutine      =   StartCoroutine(WaitTime(result));
            }
        }

        private WaitForSeconds m_WaitForSeconds;
        private Coroutine      m_Coroutine;

        private IEnumerator WaitTime(bool result)
        {
            yield return m_WaitForSeconds;
            m_Target.enabled = result;
            m_Coroutine      = null;
        }

        private new void OnDestroy()
        {
            if (m_Coroutine != null)
            {
                StopCoroutine(m_Coroutine);
            }
        }
    }
}