using UnityEngine;

namespace YIUIFramework
{
    /// <summary>
    /// 用于记录原始层，并在渲染器中可见。
    /// </summary>
    public sealed class UI3DDisplayRecord: MonoBehaviour
    {
        private int               m_Layer;
        private bool              m_Visible;
        private Renderer          m_AttachRenderer;
        private UI3DDisplayCamera m_ShowCamera;

        internal void Initialize(Renderer renderer, UI3DDisplayCamera camera)
        {
            m_AttachRenderer = renderer;
            m_ShowCamera     = camera;
            m_Layer          = renderer.gameObject.layer;
            m_Visible        = renderer.enabled;
        }

        private static bool IsParentOf(Transform obj, Transform parent)
        {
            if (obj == parent)
            {
                return true;
            }

            if (obj.parent == null)
            {
                return false;
            }

            return IsParentOf(obj.parent, parent);
        }

        private void OnTransformParentChanged()
        {
            if (m_ShowCamera == null ||
                !IsParentOf(transform, m_ShowCamera.transform))
            {
                this.SafeDestroySelf();
            }
        }

        private void OnDestroy()
        {
            if (m_AttachRenderer != null)
            {
                m_AttachRenderer.enabled = m_Visible;
            }

            gameObject.layer = m_Layer;
        }
    }
}