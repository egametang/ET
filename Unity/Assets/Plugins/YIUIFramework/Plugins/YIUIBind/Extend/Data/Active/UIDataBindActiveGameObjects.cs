using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;

namespace YIUIFramework
{
    [LabelText("任意集合的GameObject的显隐")]
    [AddComponentMenu("YIUIBind/Data/显隐 【ActiveGameObjects】 UIDataBindActiveGameObjects y")]
    public sealed class UIDataBindActiveGameObjects : UIDataBindBool
    {
        [SerializeField]
        [LabelText("控制的目标")]
        [Required("必须有此组件")]
        private GameObject[] m_Targets;

        [SerializeField]
        [LabelText("过度类型")]
        private UITransitionModeEnum m_TransitionMode = UITransitionModeEnum.Instant;

        [SerializeField]
        [LabelText("过度时间")]
        [ShowIf("m_TransitionMode", UITransitionModeEnum.Fade)]
        private float m_TransitionTime = 0.1f;

        protected override void OnValueChanged()
        {
            if (m_Targets == null)
                return;

            var result = GetResult();

            if (m_TransitionMode == UITransitionModeEnum.Instant)
            {
                SetActive(result);
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
            SetActive(result);
            m_Coroutine = null;
        }

        private void SetActive(bool set)
        {
            foreach (var value in m_Targets)
            {
                value.SetActive(set);
            }
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