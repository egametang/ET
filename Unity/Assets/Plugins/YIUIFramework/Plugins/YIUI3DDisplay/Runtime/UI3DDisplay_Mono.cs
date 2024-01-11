using System;
using UnityEngine;
using UnityEngine.UI;

namespace YIUIFramework
{
    /// <summary>
    /// 生命周期相关
    /// </summary>
    public sealed partial class UI3DDisplay
    {
        private void Awake()
        {
            m_ShowImage ??= GetComponent<RawImage>();

            if (m_ShowImage != null)
            {
                if (m_ShowObject == null)
                    m_ShowImage.enabled = false;
            }

            if (m_ShowCamera != null)
            {
                if (m_ShowObject == null)
                    m_ShowCamera.enabled = false;
                m_ShowCameraCtrl   = m_ShowCamera.GetOrAddComponent<UI3DDisplayCamera>();
                m_ShowCameraDefPos = m_ShowCamera.transform.localPosition;
            }
            else
            {
                Debug.LogError($"{this.gameObject.name} ShowCamera == null 这是不允许的 请检查 建议直接使用默认预制 不要自己修改");
            }

            g_DisPlayUIIndex += 1;
            var offsetY = g_DisPlayUIIndex * 100.0f;
            if (offsetY >= 2147)
                g_DisPlayUIIndex = 0;

            m_ModelGlobalOffset = new Vector3(0, offsetY, 0);

            if (m_MultipleTargetMode)
                InitRotationData();
        }

        private void Start()
        {
            //如果提前设置显示对象 在非多模式下 自动设置
            if (!m_MultipleTargetMode && m_ShowObject != null && m_LookCamera != null)
            {
                Show(m_ShowObject, m_LookCamera);
            }
        }

        private void OnEnable()
        {
            if (m_ShowObject == null || m_ShowTexture != null) return;

            m_ShowTexture = RenderTexture.GetTemporary(m_ResolutionX, m_ResolutionY, m_RenderTextureDepthBuffer);

            if (m_ShowImage != null)
            {
                m_ShowImage.texture = m_ShowTexture;
                m_ShowImage.enabled = true;
            }

            if (m_ShowCamera != null)
            {
                m_ShowCamera.targetTexture = m_ShowTexture;
                m_ShowCamera.enabled       = true;
            }
        }

        private void OnDisable()
        {
            if (m_ShowTexture == null) return;

            RenderTexture.ReleaseTemporary(m_ShowTexture);
            m_ShowTexture = null;

            if (m_ShowImage != null)
            {
                m_ShowImage.texture = null;
                m_ShowImage.enabled = false;
            }

            if (m_ShowCamera != null)
            {
                m_ShowCamera.targetTexture = null;
                m_ShowCamera.enabled       = false;
            }
        }

        private void OnDestroy()
        {
            if (m_ShowTexture != null)
            {
                RenderTexture.ReleaseTemporary(m_ShowTexture);
                m_ShowTexture              = null;
                m_ShowImage.texture        = null;
                m_ShowCamera.targetTexture = null;
                m_ShowCamera.enabled       = false;
            }

            DisableMeshRectShadow();

            if (m_ShowCameraCtrl != null)
            {
                m_ShowCameraCtrl.ShowObject = null;
            }
        }

        private void Update()
        {
            if (!m_ShowCamera)
                return;

            //自动同步摄像机位置旋转
            if (m_AutoSync && m_LookCamera)
            {
                var tsf = m_LookCamera.transform;
                m_ShowCamera.transform.SetPositionAndRotation(tsf.position, tsf.rotation);
            }
        }
    }
}