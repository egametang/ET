using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;
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
    [ExecuteInEditMode]
    [RequireComponent(typeof(Graphic))]
    [DisallowMultipleComponent]
    [AddComponentMenu("UI/UIEffects/UIEffect", 1)]
    public class UIEffect : BaseMaterialEffect, IMaterialModifier
    {
        private const uint k_ShaderId = 2 << 3;
        private static readonly ParameterTexture s_ParamTex = new ParameterTexture(4, 1024, "_ParamTex");

        [FormerlySerializedAs("m_ToneLevel")]
        [Tooltip("Effect factor between 0(no effect) and 1(complete effect).")]
        [SerializeField]
        [Range(0, 1)]
        float m_EffectFactor = 1;

        [Tooltip("Color effect factor between 0(no effect) and 1(complete effect).")] [SerializeField] [Range(0, 1)]
        float m_ColorFactor = 1;

        [FormerlySerializedAs("m_Blur")]
        [Tooltip("How far is the blurring from the graphic.")]
        [SerializeField]
        [Range(0, 1)]
        float m_BlurFactor = 1;

        [FormerlySerializedAs("m_ToneMode")] [Tooltip("Effect mode")] [SerializeField]
        EffectMode m_EffectMode = EffectMode.None;

        [Tooltip("Color effect mode")] [SerializeField]
        ColorMode m_ColorMode = ColorMode.Multiply;

        [Tooltip("Blur effect mode")] [SerializeField]
        BlurMode m_BlurMode = BlurMode.None;

        [Tooltip("Advanced blurring remove common artifacts in the blur effect for uGUI.")] [SerializeField]
        bool m_AdvancedBlur = false;

        private enum BlurEx
        {
            None = 0,
            Ex = 1,
        }

        /// <summary>
        /// Additional canvas shader channels to use this component.
        /// </summary>
        public AdditionalCanvasShaderChannels uvMaskChannel
        {
            get { return connector.extraChannel; }
        }

        /// <summary>
        /// Effect factor between 0(no effect) and 1(complete effect).
        /// </summary>
        public float effectFactor
        {
            get { return m_EffectFactor; }
            set
            {
                value = Mathf.Clamp(value, 0, 1);
                if (Mathf.Approximately(m_EffectFactor, value)) return;
                m_EffectFactor = value;
                SetEffectParamsDirty();
            }
        }

        /// <summary>
        /// Color effect factor between 0(no effect) and 1(complete effect).
        /// </summary>
        public float colorFactor
        {
            get { return m_ColorFactor; }
            set
            {
                value = Mathf.Clamp(value, 0, 1);
                if (Mathf.Approximately(m_ColorFactor, value)) return;
                m_ColorFactor = value;
                SetEffectParamsDirty();
            }
        }

        /// <summary>
        /// How far is the blurring from the graphic.
        /// </summary>
        public float blurFactor
        {
            get { return m_BlurFactor; }
            set
            {
                value = Mathf.Clamp(value, 0, 1);
                if (Mathf.Approximately(m_BlurFactor, value)) return;
                m_BlurFactor = value;
                SetEffectParamsDirty();
            }
        }

        /// <summary>
        /// Effect mode.
        /// </summary>
        public EffectMode effectMode
        {
            get { return m_EffectMode; }
            set
            {
                if (m_EffectMode == value) return;
                m_EffectMode = value;
                SetMaterialDirty();
            }
        }

        /// <summary>
        /// Color effect mode.
        /// </summary>
        public ColorMode colorMode
        {
            get { return m_ColorMode; }
            set
            {
                if (m_ColorMode == value) return;
                m_ColorMode = value;
                SetMaterialDirty();
            }
        }

        /// <summary>
        /// Blur effect mode(readonly).
        /// </summary>
        public BlurMode blurMode
        {
            get { return m_BlurMode; }
            set
            {
                if (m_BlurMode == value) return;
                m_BlurMode = value;
                SetMaterialDirty();
            }
        }

        /// <summary>
        /// Gets the parameter texture.
        /// </summary>
        public override ParameterTexture paramTex
        {
            get { return s_ParamTex; }
        }

        /// <summary>
        /// Advanced blurring remove common artifacts in the blur effect for uGUI.
        /// </summary>
        public bool advancedBlur
        {
            get { return m_AdvancedBlur; }
            set
            {
                if (m_AdvancedBlur == value) return;
                m_AdvancedBlur = value;
                SetVerticesDirty();
                SetMaterialDirty();
            }
        }

        public override Hash128 GetMaterialHash(Material material)
        {
            if (!isActiveAndEnabled || !material || !material.shader)
                return k_InvalidHash;

            var shaderVariantId = (uint) (((int) m_EffectMode << 6) + ((int) m_ColorMode << 9) +
                                          ((int) m_BlurMode << 11) + ((m_AdvancedBlur ? 1 : 0) << 13));
            return new Hash128(
                (uint) material.GetInstanceID(),
                k_ShaderId + shaderVariantId,
                0,
                0
            );
        }

        public override void ModifyMaterial(Material newMaterial, Graphic graphic)
        {
            var connector = GraphicConnector.FindConnector(graphic);

            newMaterial.shader = Shader.Find(string.Format("Hidden/{0} (UIEffect)", newMaterial.shader.name));
            SetShaderVariants(newMaterial, m_EffectMode, m_ColorMode, m_BlurMode,
                m_AdvancedBlur ? BlurEx.Ex : BlurEx.None);

            paramTex.RegisterMaterial(newMaterial);
        }

        /// <summary>
        /// Modifies the mesh.
        /// </summary>
        public override void ModifyMesh(VertexHelper vh, Graphic graphic)
        {
            if (!isActiveAndEnabled)
            {
                return;
            }

            var normalizedIndex = paramTex.GetNormalizedIndex(this);

            if (m_BlurMode != BlurMode.None && advancedBlur)
            {
                vh.GetUIVertexStream(s_TempVerts);
                vh.Clear();
                var count = s_TempVerts.Count;

                // Bundle
                int bundleSize = connector.IsText(graphic) ? 6 : count;
                Rect posBounds = default(Rect);
                Rect uvBounds = default(Rect);
                Vector3 size = default(Vector3);
                Vector3 tPos = default(Vector3);
                Vector3 tUV = default(Vector3);
                float expand = (float) blurMode * 6 * 2;

                for (int i = 0; i < count; i += bundleSize)
                {
                    // min/max for bundled-quad
                    GetBounds(s_TempVerts, i, bundleSize, ref posBounds, ref uvBounds, true);

                    // Pack uv mask.
                    Vector2 uvMask = new Vector2(Packer.ToFloat(uvBounds.xMin, uvBounds.yMin),
                        Packer.ToFloat(uvBounds.xMax, uvBounds.yMax));

                    // Quad
                    for (int j = 0; j < bundleSize; j += 6)
                    {
                        Vector3 cornerPos1 = s_TempVerts[i + j + 1].position;
                        Vector3 cornerPos2 = s_TempVerts[i + j + 4].position;

                        // Is outer quad?
                        bool hasOuterEdge = (bundleSize == 6)
                                            || !posBounds.Contains(cornerPos1)
                                            || !posBounds.Contains(cornerPos2);
                        if (hasOuterEdge)
                        {
                            Vector3 cornerUv1 = s_TempVerts[i + j + 1].uv0;
                            Vector3 cornerUv2 = s_TempVerts[i + j + 4].uv0;

                            Vector3 centerPos = (cornerPos1 + cornerPos2) / 2;
                            Vector3 centerUV = (cornerUv1 + cornerUv2) / 2;
                            size = (cornerPos1 - cornerPos2);

                            size.x = 1 + expand / Mathf.Abs(size.x);
                            size.y = 1 + expand / Mathf.Abs(size.y);
                            size.z = 1 + expand / Mathf.Abs(size.z);

                            tPos = centerPos - Vector3.Scale(size, centerPos);
                            tUV = centerUV - Vector3.Scale(size, centerUV);
                        }

                        // Vertex
                        for (int k = 0; k < 6; k++)
                        {
                            UIVertex vt = s_TempVerts[i + j + k];

                            Vector3 pos = vt.position;
                            Vector2 uv0 = vt.uv0;

                            if (hasOuterEdge && (pos.x < posBounds.xMin || posBounds.xMax < pos.x))
                            {
                                pos.x = pos.x * size.x + tPos.x;
                                uv0.x = uv0.x * size.x + tUV.x;
                            }

                            if (hasOuterEdge && (pos.y < posBounds.yMin || posBounds.yMax < pos.y))
                            {
                                pos.y = pos.y * size.y + tPos.y;
                                uv0.y = uv0.y * size.y + tUV.y;
                            }

                            vt.uv0 = new Vector2(Packer.ToFloat((uv0.x + 0.5f) / 2f, (uv0.y + 0.5f) / 2f),
                                normalizedIndex);
                            vt.position = pos;

                            connector.SetExtraChannel(ref vt, uvMask);

                            s_TempVerts[i + j + k] = vt;
                        }
                    }
                }

                vh.AddUIVertexTriangleStream(s_TempVerts);
                s_TempVerts.Clear();
            }
            else
            {
                int count = vh.currentVertCount;
                UIVertex vt = default(UIVertex);
                for (int i = 0; i < count; i++)
                {
                    vh.PopulateUIVertex(ref vt, i);
                    Vector2 uv0 = vt.uv0;
                    vt.uv0 = new Vector2(
                        Packer.ToFloat((uv0.x + 0.5f) / 2f, (uv0.y + 0.5f) / 2f),
                        normalizedIndex
                    );
                    vh.SetUIVertex(vt, i);
                }
            }
        }

        protected override void SetEffectParamsDirty()
        {
            paramTex.SetData(this, 0, m_EffectFactor); // param.x : effect factor
            paramTex.SetData(this, 1, m_ColorFactor); // param.y : color factor
            paramTex.SetData(this, 2, m_BlurFactor); // param.z : blur factor
        }

        static void GetBounds(List<UIVertex> verts, int start, int count, ref Rect posBounds, ref Rect uvBounds,
            bool global)
        {
            Vector2 minPos = new Vector2(float.MaxValue, float.MaxValue);
            Vector2 maxPos = new Vector2(float.MinValue, float.MinValue);
            Vector2 minUV = new Vector2(float.MaxValue, float.MaxValue);
            Vector2 maxUV = new Vector2(float.MinValue, float.MinValue);
            for (int i = start; i < start + count; i++)
            {
                UIVertex vt = verts[i];

                Vector2 uv = vt.uv0;
                Vector3 pos = vt.position;

                // Left-Bottom
                if (minPos.x >= pos.x && minPos.y >= pos.y)
                {
                    minPos = pos;
                }
                // Right-Top
                else if (maxPos.x <= pos.x && maxPos.y <= pos.y)
                {
                    maxPos = pos;
                }

                // Left-Bottom
                if (minUV.x >= uv.x && minUV.y >= uv.y)
                {
                    minUV = uv;
                }
                // Right-Top
                else if (maxUV.x <= uv.x && maxUV.y <= uv.y)
                {
                    maxUV = uv;
                }
            }

            // Shrink coordinate for detect edge
            posBounds.Set(minPos.x + 0.001f, minPos.y + 0.001f, maxPos.x - minPos.x - 0.002f,
                maxPos.y - minPos.y - 0.002f);
            uvBounds.Set(minUV.x, minUV.y, maxUV.x - minUV.x, maxUV.y - minUV.y);
        }
    }
}
