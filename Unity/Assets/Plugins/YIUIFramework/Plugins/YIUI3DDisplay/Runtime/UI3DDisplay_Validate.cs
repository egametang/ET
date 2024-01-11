using System;
using UnityEngine;

namespace YIUIFramework
{
    public sealed partial class UI3DDisplay
    {
        #if UNITY_EDITOR
        private void OnValidate()
        {
            if (m_AutoChangeSize)
            {
                var rect = transform.GetComponent<RectTransform>();
                if (Math.Abs(rect.sizeDelta.x - m_ResolutionX) > 0.01f ||
                    Math.Abs(rect.sizeDelta.y - m_ResolutionY) > 0.01f)
                    rect.sizeDelta = new Vector2(m_ResolutionX, m_ResolutionY);
            }

            ChangeLayerName(m_ShowLayerName);
        }
        #endif
    }
}