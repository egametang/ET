using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;


namespace Coffee.UIEffects
{
    /// <summary>
    /// HSV Modifier.
    /// </summary>
    [AddComponentMenu("UI/UIEffects/UIHsvModifier", 4)]
    public class UIHsvModifier : BaseMaterialEffect
    {
        private const uint k_ShaderId = 6 << 3;
        private static readonly ParameterTexture s_ParamTex = new ParameterTexture(7, 128, "_ParamTex");

        [Header("Target")] [Tooltip("Target color to affect hsv shift.")] [SerializeField] [ColorUsage(false)]
        Color m_TargetColor = Color.red;

        [Tooltip("Color range to affect hsv shift [0 ~ 1].")] [SerializeField] [Range(0, 1)]
        float m_Range = 0.1f;

        [Header("Adjustment")] [Tooltip("Hue shift [-0.5 ~ 0.5].")] [SerializeField] [Range(-0.5f, 0.5f)]
        float m_Hue;

        [Tooltip("Saturation shift [-0.5 ~ 0.5].")] [SerializeField] [Range(-0.5f, 0.5f)]
        float m_Saturation;

        [Tooltip("Value shift [-0.5 ~ 0.5].")] [SerializeField] [Range(-0.5f, 0.5f)]
        float m_Value;

        /// <summary>
        /// Target color to affect hsv shift.
        /// </summary>
        public Color targetColor
        {
            get { return m_TargetColor; }
            set
            {
                if (m_TargetColor == value) return;
                m_TargetColor = value;
                SetEffectParamsDirty();
            }
        }

        /// <summary>
        /// Color range to affect hsv shift [0 ~ 1].
        /// </summary>
        public float range
        {
            get { return m_Range; }
            set
            {
                value = Mathf.Clamp(value, 0, 1);
                if (Mathf.Approximately(m_Range, value)) return;
                m_Range = value;
                SetEffectParamsDirty();
            }
        }

        /// <summary>
        /// Saturation shift [-0.5 ~ 0.5].
        /// </summary>
        public float saturation
        {
            get { return m_Saturation; }
            set
            {
                value = Mathf.Clamp(value, -0.5f, 0.5f);
                if (Mathf.Approximately(m_Saturation, value)) return;
                m_Saturation = value;
                SetEffectParamsDirty();
            }
        }

        /// <summary>
        /// Value shift [-0.5 ~ 0.5].
        /// </summary>
        public float value
        {
            get { return m_Value; }
            set
            {
                value = Mathf.Clamp(value, -0.5f, 0.5f);
                if (Mathf.Approximately(m_Value, value)) return;
                m_Value = value;
                SetEffectParamsDirty();
            }
        }

        /// <summary>
        /// Hue shift [-0.5 ~ 0.5].
        /// </summary>
        public float hue
        {
            get { return m_Hue; }
            set
            {
                value = Mathf.Clamp(value, -0.5f, 0.5f);
                if (Mathf.Approximately(m_Hue, value)) return;
                m_Hue = value;
                SetEffectParamsDirty();
            }
        }

        /// <summary>
        /// Gets the parameter texture.
        /// </summary>
        public override ParameterTexture paramTex
        {
            get { return s_ParamTex; }
        }

        public override Hash128 GetMaterialHash(Material material)
        {
            if (!isActiveAndEnabled || !material || !material.shader)
                return k_InvalidHash;

            return new Hash128(
                (uint) material.GetInstanceID(),
                k_ShaderId,
                0,
                0
            );
        }

        public override void ModifyMaterial(Material newMaterial, Graphic graphic)
        {
            var connector = GraphicConnector.FindConnector(graphic);

            newMaterial.shader = Shader.Find(string.Format("Hidden/{0} (UIHsvModifier)", newMaterial.shader.name));
            paramTex.RegisterMaterial(newMaterial);
        }

        public override void ModifyMesh(VertexHelper vh, Graphic graphic)
        {
            if (!isActiveAndEnabled)
                return;

            var normalizedIndex = paramTex.GetNormalizedIndex(this);
            var vertex = default(UIVertex);
            var count = vh.currentVertCount;
            for (var i = 0; i < count; i++)
            {
                vh.PopulateUIVertex(ref vertex, i);

                vertex.uv0 = new Vector2(
                    Packer.ToFloat(vertex.uv0.x, vertex.uv0.y),
                    normalizedIndex
                );
                vh.SetUIVertex(vertex, i);
            }
        }

        protected override void SetEffectParamsDirty()
        {
            float h, s, v;
            Color.RGBToHSV(m_TargetColor, out h, out s, out v);

            paramTex.SetData(this, 0, h); // param1.x : target hue
            paramTex.SetData(this, 1, s); // param1.y : target saturation
            paramTex.SetData(this, 2, v); // param1.z : target value
            paramTex.SetData(this, 3, m_Range); // param1.w : target range
            paramTex.SetData(this, 4, m_Hue + 0.5f); // param2.x : hue shift
            paramTex.SetData(this, 5, m_Saturation + 0.5f); // param2.y : saturation shift
            paramTex.SetData(this, 6, m_Value + 0.5f); // param2.z : value shift
        }
    }
}
