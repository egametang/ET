using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using System.Collections;

namespace Coffee.UIEffects
{
    /// <summary>
    /// UIEffect.
    /// </summary>
    [AddComponentMenu("UI/UIEffects/UIShiny", 2)]
    public class UIShiny : BaseMaterialEffect
    {
        private const uint k_ShaderId = 1 << 3;
        private static readonly ParameterTexture s_ParamTex = new ParameterTexture(8, 128, "_ParamTex");

        float _lastRotation;
        EffectArea _lastEffectArea;

        [Tooltip("Location for shiny effect.")] [FormerlySerializedAs("m_Location")] [SerializeField] [Range(0, 1)]
        float m_EffectFactor = 0.5f;

        [Tooltip("Width for shiny effect.")] [SerializeField] [Range(0, 1)]
        float m_Width = 0.25f;

        [Tooltip("Rotation for shiny effect.")] [SerializeField] [Range(-180, 180)]
        float m_Rotation = 135;

        [Tooltip("Softness for shiny effect.")] [SerializeField] [Range(0.01f, 1)]
        float m_Softness = 1f;

        [Tooltip("Brightness for shiny effect.")] [FormerlySerializedAs("m_Alpha")] [SerializeField] [Range(0, 1)]
        float m_Brightness = 1f;

        [Tooltip("Gloss factor for shiny effect.")] [FormerlySerializedAs("m_Highlight")] [SerializeField] [Range(0, 1)]
        float m_Gloss = 1;

        [Header("Advanced Option")] [Tooltip("The area for effect.")] [SerializeField]
        protected EffectArea m_EffectArea;

        [SerializeField] EffectPlayer m_Player;

        /// <summary>
        /// Effect factor between 0(start) and 1(end).
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
        /// Width for shiny effect.
        /// </summary>
        public float width
        {
            get { return m_Width; }
            set
            {
                value = Mathf.Clamp(value, 0, 1);
                if (Mathf.Approximately(m_Width, value)) return;
                m_Width = value;
                SetEffectParamsDirty();
            }
        }

        /// <summary>
        /// Softness for shiny effect.
        /// </summary>
        public float softness
        {
            get { return m_Softness; }
            set
            {
                value = Mathf.Clamp(value, 0.01f, 1);
                if (Mathf.Approximately(m_Softness, value)) return;
                m_Softness = value;
                SetEffectParamsDirty();
            }
        }

        /// <summary>
        /// Brightness for shiny effect.
        /// </summary>
        public float brightness
        {
            get { return m_Brightness; }
            set
            {
                value = Mathf.Clamp(value, 0, 1);
                if (Mathf.Approximately(m_Brightness, value)) return;
                m_Brightness = value;
                SetEffectParamsDirty();
            }
        }

        /// <summary>
        /// Gloss factor for shiny effect.
        /// </summary>
        public float gloss
        {
            get { return m_Gloss; }
            set
            {
                value = Mathf.Clamp(value, 0, 1);
                if (Mathf.Approximately(m_Gloss, value)) return;
                m_Gloss = value;
                SetEffectParamsDirty();
            }
        }

        /// <summary>
        /// Rotation for shiny effect.
        /// </summary>
        public float rotation
        {
            get { return m_Rotation; }
            set
            {
                if (Mathf.Approximately(m_Rotation, value)) return;
                m_Rotation = value;
                SetVerticesDirty();
            }
        }

        /// <summary>
        /// The area for effect.
        /// </summary>
        public EffectArea effectArea
        {
            get { return m_EffectArea; }
            set
            {
                if (m_EffectArea == value) return;
                m_EffectArea = value;
                SetVerticesDirty();
            }
        }

        /// <summary>
        /// Gets the parameter texture.
        /// </summary>
        public override ParameterTexture paramTex
        {
            get { return s_ParamTex; }
        }

        public EffectPlayer effectPlayer
        {
            get { return m_Player ?? (m_Player = new EffectPlayer()); }
        }

        /// <summary>
        /// This function is called when the object becomes enabled and active.
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();
            effectPlayer.OnEnable(f => effectFactor = f);
        }

        /// <summary>
        /// This function is called when the behaviour becomes disabled () or inactive.
        /// </summary>
        protected override void OnDisable()
        {
            base.OnDisable();
            effectPlayer.OnDisable();
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

            newMaterial.shader = Shader.Find(string.Format("Hidden/{0} (UIShiny)", newMaterial.shader.name));
            paramTex.RegisterMaterial(newMaterial);
        }

        /// <summary>
        /// Modifies the mesh.
        /// </summary>
        public override void ModifyMesh(VertexHelper vh, Graphic graphic)
        {
            if (!isActiveAndEnabled)
                return;

            var normalizedIndex = paramTex.GetNormalizedIndex(this);
            var rect = m_EffectArea.GetEffectArea(vh, rectTransform.rect);

            // rotation.
            var rad = m_Rotation * Mathf.Deg2Rad;
            var dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
            dir.x *= rect.height / rect.width;
            dir = dir.normalized;

            // Calculate vertex position.
            var vertex = default(UIVertex);
            var localMatrix = new Matrix2x3(rect, dir.x, dir.y); // Get local matrix.
            for (int i = 0; i < vh.currentVertCount; i++)
            {
                vh.PopulateUIVertex(ref vertex, i);
                Vector2 normalizedPos;
                connector.GetNormalizedFactor(m_EffectArea, i, localMatrix, vertex.position, out normalizedPos);

                vertex.uv0 = new Vector2(
                    Packer.ToFloat(vertex.uv0.x, vertex.uv0.y),
                    Packer.ToFloat(normalizedPos.y, normalizedIndex)
                );

                vh.SetUIVertex(vertex, i);
            }
        }

        /// <summary>
        /// Play effect.
        /// </summary>
        public void Play(bool reset = true)
        {
            effectPlayer.Play(reset);
        }

        /// <summary>
        /// Stop effect.
        /// </summary>
        public void Stop(bool reset = true)
        {
            effectPlayer.Stop(reset);
        }

        protected override void SetEffectParamsDirty()
        {
            paramTex.SetData(this, 0, m_EffectFactor); // param1.x : location
            paramTex.SetData(this, 1, m_Width); // param1.y : width
            paramTex.SetData(this, 2, m_Softness); // param1.z : softness
            paramTex.SetData(this, 3, m_Brightness); // param1.w : blightness
            paramTex.SetData(this, 4, m_Gloss); // param2.x : gloss
        }

        protected override void SetVerticesDirty()
        {
            base.SetVerticesDirty();

            _lastRotation = m_Rotation;
            _lastEffectArea = m_EffectArea;
        }

        protected override void OnDidApplyAnimationProperties()
        {
            base.OnDidApplyAnimationProperties();

            if (!Mathf.Approximately(_lastRotation, m_Rotation)
                || _lastEffectArea != m_EffectArea)
                SetVerticesDirty();
        }
    }
}
