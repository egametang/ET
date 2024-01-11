using ET;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using YIUIFramework;
using Logger = YIUIFramework.Logger;

namespace YIUIFramework
{
    [RequireComponent(typeof(Image))]
    [LabelText("Image 图片")]
    [AddComponentMenu("YIUIBind/Data/图片 【Image】 UIDataBindImage")]
    public sealed class UIDataBindImage : UIDataBindSelectBase
    {
        [SerializeField]
        [ReadOnly]
        [Required("必须有此组件")]
        [LabelText("图片")]
        private Image m_Image;

        [SerializeField]
        [LabelText("自动调整图像大小")]
        private bool m_SetNativeSize = false;

        [SerializeField]
        [LabelText("可修改Enabled")]
        private bool m_ChangeEnabled = true;

        private string m_LastSpriteName;

        protected override int Mask()
        {
            return 1 << (int)EUIBindDataType.String;
        }

        protected override int SelectMax()
        {
            return 1;
        }

        protected override void OnRefreshData()
        {
            base.OnRefreshData();
            m_Image ??= GetComponent<Image>();
            if (!m_ChangeEnabled && !m_Image.enabled)
            {
                Logger.LogError($"{name} 当前禁止修改Enabled 且当前处于隐藏状态 可能会出现问题 请检查");
            }
        }

        private void SetEnabled(bool set)
        {
            if (!m_ChangeEnabled) return;

            if (m_Image == null) return;

            m_Image.enabled = set;
        }

        protected override void OnValueChanged()
        {
            if (m_Image == null) return;

            var dataValue = GetFirstValue<string>();

            if (string.IsNullOrEmpty(dataValue))
            {
                SetEnabled(false);
                return;
            }
            
            if (m_LastSpriteName == dataValue)
            {
                SetEnabled(true);
                return;
            }

            m_LastSpriteName = dataValue;

            if (!UIOperationHelper.IsPlaying())
            {
                return;
            }

            ChangeSprite(dataValue).Coroutine();
        }

        private async ETTask ChangeSprite(string resName)
        {
            ReleaseLastSprite();
            
            #if UNITY_EDITOR
            if (!YIUILoadHelper.VerifyAssetValidity(resName))
            {
                Logger.LogError($"没有这个资源 图片无法加载 请检查 {resName}");
                SetEnabled(false);
                return;
            }            
            #endif
            
            var sprite = await YIUILoadHelper.LoadAssetAsync<Sprite>(resName);
            
            if (m_Image != null && sprite != null)
            {
                m_LastSprite   = sprite;
                m_Image.sprite = sprite;
                if (m_SetNativeSize)
                    m_Image.SetNativeSize();
                
                SetEnabled(true);
            }
            else
            {
                if (m_Image == null)
                    Logger.LogError($"{this.gameObject.name} m_Image == null");
                if (sprite == null)
                    Logger.LogError($"没有这个资源 图片无法加载 请检查 {resName}");
                SetEnabled(false);
            }
        }

        protected override void UnBindData()
        {
            base.UnBindData();
            if (!UIOperationHelper.IsPlaying())
            {
                return;
            }
            ReleaseLastSprite();
        }

        private Sprite m_LastSprite;

        private void ReleaseLastSprite()
        {
            if (m_LastSprite != null)
            {
                YIUILoadHelper.Release(m_LastSprite);
                m_LastSprite = null;
            }
        }
    }
}