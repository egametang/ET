using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
#if UNITY_EDITOR
using System.IO;
using System.Linq;
using UnityEditor;

#endif

namespace Coffee.UIEffects
{
    /// <summary>
    /// UIEffect.
    /// </summary>
    [RequireComponent(typeof(Graphic))]
    [AddComponentMenu("UI/UIEffects/UIShadow", 100)]
    public class UIShadow : BaseMeshEffect, IParameterTexture
    {
        static readonly List<UIShadow> tmpShadows = new List<UIShadow>();
        static readonly List<UIVertex> s_Verts = new List<UIVertex>(4096);

        int _graphicVertexCount;
        UIEffect _uiEffect;

        [Tooltip("How far is the blurring shadow from the graphic.")]
        [FormerlySerializedAs("m_Blur")]
        [SerializeField]
        [Range(0, 1)]
        float m_BlurFactor = 1;

        [Tooltip("Shadow effect style.")] [SerializeField]
        ShadowStyle m_Style = ShadowStyle.Shadow;

        [SerializeField] private Color m_EffectColor = new Color(0f, 0f, 0f, 0.5f);

        [SerializeField] private Vector2 m_EffectDistance = new Vector2(1f, -1f);

        [SerializeField] private bool m_UseGraphicAlpha = true;

        private const float kMaxEffectDistance = 600f;

        public Color effectColor
        {
            get { return m_EffectColor; }
            set
            {
                if (m_EffectColor == value) return;
                m_EffectColor = value;
                SetVerticesDirty();
            }
        }

        public Vector2 effectDistance
        {
            get { return m_EffectDistance; }
            set
            {
                if (value.x > kMaxEffectDistance)
                    value.x = kMaxEffectDistance;
                if (value.x < -kMaxEffectDistance)
                    value.x = -kMaxEffectDistance;

                if (value.y > kMaxEffectDistance)
                    value.y = kMaxEffectDistance;
                if (value.y < -kMaxEffectDistance)
                    value.y = -kMaxEffectDistance;

                if (m_EffectDistance == value) return;
                m_EffectDistance = value;
                SetEffectParamsDirty();
            }
        }

        public bool useGraphicAlpha
        {
            get { return m_UseGraphicAlpha; }
            set
            {
                if (m_UseGraphicAlpha == value) return;
                m_UseGraphicAlpha = value;
                SetEffectParamsDirty();
            }
        }

        /// <summary>
        /// How far is the blurring shadow from the graphic.
        /// </summary>
        public float blurFactor
        {
            get { return m_BlurFactor; }
            set
            {
                value = Mathf.Clamp(value, 0, 2);
                if (Mathf.Approximately(m_BlurFactor, value)) return;
                m_BlurFactor = value;
                SetEffectParamsDirty();
            }
        }

        /// <summary>
        /// Shadow effect style.
        /// </summary>
        public ShadowStyle style
        {
            get { return m_Style; }
            set
            {
                if (m_Style == value) return;
                m_Style = value;
                SetEffectParamsDirty();
            }
        }

        /// <summary>
        /// Gets or sets the parameter index.
        /// </summary>
        public int parameterIndex { get; set; }

        /// <summary>
        /// Gets the parameter texture.
        /// </summary>
        public ParameterTexture paramTex { get; private set; }

        protected override void OnEnable()
        {
            base.OnEnable();

            _uiEffect = GetComponent<UIEffect>();
            if (!_uiEffect) return;

            paramTex = _uiEffect.paramTex;
            paramTex.Register(this);
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            _uiEffect = null;
            if (paramTex == null) return;

            paramTex.Unregister(this);
            paramTex = null;
        }


// #if UNITY_EDITOR
//         protected override void OnValidate()
//         {
//             effectDistance = m_EffectDistance;
//             base.OnValidate();
//         }
// #endif

        // #if TMP_PRESENT
        // protected void OnCullStateChanged (bool state)
        // {
        // 	SetVerticesDirty ();
        // }
        //
        // Vector2 res;
        // protected override void LateUpdate ()
        // {
        // 	if (res.x != Screen.width || res.y != Screen.height)
        // 	{
        // 		res.x = Screen.width;
        // 		res.y = Screen.height;
        // 		SetVerticesDirty ();
        // 	}
        // 	if (textMeshPro && transform.hasChanged)
        // 	{
        // 		transform.hasChanged = false;
        // 	}
        // 	base.LateUpdate ();
        // }
        // #endif

        /// <summary>
        /// Modifies the mesh.
        /// </summary>
        public override void ModifyMesh(VertexHelper vh, Graphic graphic)
        {
            if (!isActiveAndEnabled || vh.currentVertCount <= 0 || m_Style == ShadowStyle.None)
            {
                return;
            }

            vh.GetUIVertexStream(s_Verts);

            GetComponents<UIShadow>(tmpShadows);

            foreach (var s in tmpShadows)
            {
                if (!s.isActiveAndEnabled) continue;
                if (s == this)
                {
                    foreach (var s2 in tmpShadows)
                    {
                        s2._graphicVertexCount = s_Verts.Count;
                    }
                }

                break;
            }

            tmpShadows.Clear();

            //================================
            // Append shadow vertices.
            //================================
            {
                _uiEffect = _uiEffect ? _uiEffect : GetComponent<UIEffect>();
                var start = s_Verts.Count - _graphicVertexCount;
                var end = s_Verts.Count;

                if (paramTex != null && _uiEffect && _uiEffect.isActiveAndEnabled)
                {
                    paramTex.SetData(this, 0, _uiEffect.effectFactor); // param.x : effect factor
                    paramTex.SetData(this, 1, 255); // param.y : color factor
                    paramTex.SetData(this, 2, m_BlurFactor); // param.z : blur factor
                }

                ApplyShadow(s_Verts, effectColor, ref start, ref end, effectDistance, style, useGraphicAlpha);
            }

            vh.Clear();
            vh.AddUIVertexTriangleStream(s_Verts);

            s_Verts.Clear();
        }

        /// <summary>
        /// Append shadow vertices.
        /// * It is similar to Shadow component implementation.
        /// </summary>
        private void ApplyShadow(List<UIVertex> verts, Color color, ref int start, ref int end, Vector2 distance,
            ShadowStyle style, bool alpha)
        {
            if (style == ShadowStyle.None || color.a <= 0)
                return;

            var x = distance.x;
            var y = distance.y;
            // Append Shadow.
            ApplyShadowZeroAlloc(verts, color, ref start, ref end, x, y, alpha);

            switch (style)
            {
                // Append Shadow3.
                case ShadowStyle.Shadow3:
                    ApplyShadowZeroAlloc(verts, color, ref start, ref end, x, 0, alpha);
                    ApplyShadowZeroAlloc(verts, color, ref start, ref end, 0, y, alpha);
                    break;
                // Append Outline.
                case ShadowStyle.Outline:
                    ApplyShadowZeroAlloc(verts, color, ref start, ref end, x, -y, alpha);
                    ApplyShadowZeroAlloc(verts, color, ref start, ref end, -x, y, alpha);
                    ApplyShadowZeroAlloc(verts, color, ref start, ref end, -x, -y, alpha);
                    break;
                // Append Outline8.
                case ShadowStyle.Outline8:
                    ApplyShadowZeroAlloc(verts, color, ref start, ref end, x, -y, alpha);
                    ApplyShadowZeroAlloc(verts, color, ref start, ref end, -x, y, alpha);
                    ApplyShadowZeroAlloc(verts, color, ref start, ref end, -x, -y, alpha);
                    ApplyShadowZeroAlloc(verts, color, ref start, ref end, -x, 0, alpha);
                    ApplyShadowZeroAlloc(verts, color, ref start, ref end, 0, -y, alpha);
                    ApplyShadowZeroAlloc(verts, color, ref start, ref end, x, 0, alpha);
                    ApplyShadowZeroAlloc(verts, color, ref start, ref end, 0, y, alpha);
                    break;
            }
        }

        /// <summary>
        /// Append shadow vertices.
        /// * It is similar to Shadow component implementation.
        /// </summary>
        private void ApplyShadowZeroAlloc(List<UIVertex> verts, Color color, ref int start, ref int end, float x,
            float y, bool alpha)
        {
            // Check list capacity.
            var count = end - start;
            var neededCapacity = verts.Count + count;
            if (verts.Capacity < neededCapacity)
                verts.Capacity *= 2;

            var normalizedIndex = paramTex != null && _uiEffect && _uiEffect.isActiveAndEnabled
                ? paramTex.GetNormalizedIndex(this)
                : -1;

            // Add
            var vt = default(UIVertex);
            for (var i = 0; i < count; i++)
            {
                verts.Add(vt);
            }

            // Move
            for (var i = verts.Count - 1; count <= i; i--)
            {
                verts[i] = verts[i - count];
            }

            // Append shadow vertices to the front of list.
            // * The original vertex is pushed backward.
            for (var i = 0; i < count; ++i)
            {
                vt = verts[i + start + count];

                var v = vt.position;
                vt.position.Set(v.x + x, v.y + y, v.z);

                var vertColor = effectColor;
                vertColor.a = alpha ? color.a * vt.color.a / 255 : color.a;
                vt.color = vertColor;


                // Set UIEffect parameters
                if (0 <= normalizedIndex)
                {
                    vt.uv0 = new Vector2(
                        vt.uv0.x,
                        normalizedIndex
                    );
                }

                verts[i] = vt;
            }

            // Update next shadow offset.
            start = end;
            end = verts.Count;
        }

        /// <summary>
        /// Mark the UIEffect as dirty.
        /// </summary>
        // void _SetDirty()
        // {
        //     if (graphic)
        //         graphic.SetVerticesDirty();
        // }

// #if UNITY_EDITOR
//         public void OnBeforeSerialize()
//         {
//         }
//
//         public void OnAfterDeserialize()
//         {
//             EditorApplication.delayCall += UpgradeIfNeeded;
//         }
//
//
// #pragma warning disable 0612
//         void UpgradeIfNeeded()
//         {
//             if (0 < m_AdditionalShadows.Count)
//             {
//                 foreach (var s in m_AdditionalShadows)
//                 {
//                     if (s.style == ShadowStyle.None)
//                     {
//                         continue;
//                     }
//
//                     var shadow = gameObject.AddComponent<UIShadow>();
//                     shadow.style = s.style;
//                     shadow.effectDistance = s.effectDistance;
//                     shadow.effectColor = s.effectColor;
//                     shadow.useGraphicAlpha = s.useGraphicAlpha;
//                     shadow.blurFactor = s.blur;
//                 }
//
//                 m_AdditionalShadows = null;
//             }
//         }
// #pragma warning restore 0612
// #endif
    }
}
