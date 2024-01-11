using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using UnityEngine.UI;

namespace YIUIFramework
{
    /// <summary>
    /// 点击按钮影响组件大小
    /// </summary>
    public class YIUIClickEffect : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [Tooltip("被影响的目标")]
        public RectTransform targetTsf;

        [Tooltip("变化大小 (倍数)")]
        public float scaleValue = 0.9f;

        [Tooltip("变小时间")]
        public float scaleTime = 0;

        [Tooltip("变大时间")]
        public float popTime = 0;

        private Button m_button;

        private Vector3 targetScale; //目标大小
        private Vector3 atScale;     //当前大小

        /// <summary>
        /// 可调整动画状态
        /// </summary>
        public Ease ease = Ease.OutElastic;

        private void Awake()
        {
            m_button = GetComponent<Button>(); //需要先挂button 否则无效
            if (targetTsf == null)             //如果没有目标则默认自己为目标
            {
                targetTsf = transform.gameObject.GetComponent<RectTransform>();
            }

            atScale     = targetTsf.localScale;
            targetScale = atScale * scaleValue;
        }

        private void OnDestroy()
        {
            targetTsf.DOKill();
        }

        //按下
        public void OnPointerDown(PointerEventData eventData)
        {
            if (m_button)
            {
                if (m_button.enabled && m_button.interactable)
                {
                    targetTsf.DOScale(targetScale, scaleTime).SetEase(ease);
                }
            }
            else
            {
                targetTsf.DOScale(targetScale, scaleTime).SetEase(ease);
            }
        }

        //抬起
        public void OnPointerUp(PointerEventData eventData)
        {
            targetTsf.DOKill();
            targetTsf.DOScale(atScale, popTime).SetEase(ease); //回到本来大小
        }

        #if UNITY_EDITOR
        private void OnValidate()
        {
            if (targetTsf == null) //如果没有目标则默认自己为目标
            {
                targetTsf = transform.gameObject.GetComponent<RectTransform>();
            }
        }
        #endif
    }
}