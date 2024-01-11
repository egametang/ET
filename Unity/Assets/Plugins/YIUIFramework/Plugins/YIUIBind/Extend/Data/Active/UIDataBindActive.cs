using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;

namespace YIUIFramework
{
    [LabelText("GameObject的显隐")]
    [AddComponentMenu("YIUIBind/Data/显隐 【Active】 UIDataBindActive")]
    public sealed class UIDataBindActive : UIDataBindBool
    {
        [SerializeField]
        [LabelText("过度类型")]
        private UITransitionModeEnum m_TransitionMode = UITransitionModeEnum.Instant;

        [SerializeField]
        [LabelText("过度时间")]
        [ShowIf("m_TransitionMode", UITransitionModeEnum.Fade)]
        private float m_TransitionTime = 0.1f;

        private CanvasGroup m_CanvasGroup;

        protected override void OnValueChanged()
        {
            if (gameObject == null)
                return;

            var result = GetResult();

            if (m_TransitionMode == UITransitionModeEnum.Instant)
            {
                gameObject.SetActive(result);
            }
            else
            {
                if (m_CanvasGroup == null)
                {
                    m_CanvasGroup =
                        gameObject.GetComponent<CanvasGroup>();
                }

                if (m_CanvasGroup != null)
                {
                    gameObject.SetActive(true);
                    if (gameObject.activeInHierarchy)
                    {
                        StopAllCoroutines();
                        if (result)
                        {
                            StartCoroutine(TransitionFade(
                                m_CanvasGroup, 1.0f, true));
                        }
                        else
                        {
                            StartCoroutine(TransitionFade(
                                m_CanvasGroup, 0.0f, false));
                        }
                    }
                    else
                    {
                        gameObject.SetActive(result);
                    }
                }
                else
                {
                    gameObject.SetActive(result);
                }
            }
        }

        private IEnumerator TransitionFade(
            CanvasGroup group, float alphaTarget, bool activeTarget)
        {
            var leftTime   = m_TransitionTime;
            var alphaStart = group.alpha;
            while (leftTime > 0.0f)
            {
                yield return null;
                leftTime -= Time.deltaTime;
                var alpha = Mathf.Lerp(
                    alphaStart,
                    alphaTarget,
                    1.0f - (leftTime / m_TransitionTime));
                group.alpha = alpha;
            }

            group.gameObject.SetActive(activeTarget);
        }
    }
}