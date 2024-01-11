using System.Collections.Generic;
using UnityEngine;

namespace YIUIFramework
{
    public sealed class UI3DDisplayCamera: MonoBehaviour
    {
        private GameObject m_ShowObject;

        internal GameObject ShowObject
        {
            get => m_ShowObject;
            set
            {
                if (m_ShowObject != null &&
                    m_ShowObject != value)
                {
                    ResetRenderer(m_ShowObject.transform);
                }

                m_ShowObject = value;
                if (value != null)
                    SetupRenderer(value.transform);
            }
        }

        private int m_ShowLayer = 0;

        internal int ShowLayer
        {
            get => m_ShowLayer;
            set
            {
                if (m_ShowLayer != value)
                {
                    m_ShowLayer = value;
                    if (ShowObject != null)
                        SetupRenderer(ShowObject.transform);
                }
            }
        }

        private static void ResetRenderer(Transform transform)
        {
            var renderers = ListPool<Renderer>.Get();
            transform.GetComponentsInChildren(true, renderers);
            foreach (var renderer in renderers)
                renderer.gameObject.layer = 0;
            ListPool<Renderer>.Put(renderers);
        }

        public void SetupRenderer(Transform transform)
        {
            if (ShowObject == null) return;

            var renderers = ListPool<Renderer>.Get();
            transform.GetComponentsInChildren(true, renderers);
            foreach (var renderer in renderers)
                renderer.gameObject.layer = ShowLayer;
            ListPool<Renderer>.Put(renderers);
        }

        private void OnEnable()
        {
            if (ShowObject != null)
            {
                SetupRenderer(ShowObject.transform);
            }
        }

        private void OnDisable()
        {
            if (ShowObject != null)
            {
                ResetRenderer(ShowObject.transform);
            }
        }
    }
}