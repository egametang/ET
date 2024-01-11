using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace Coffee.UIEffects
{
    /// <summary>
    /// Dissolve effect for uGUI.
    /// </summary>
    [ExecuteInEditMode]
    public class UISyncEffect : BaseMaterialEffect
    {
        [Tooltip("The target effect to synchronize.")] [SerializeField]
        private BaseMeshEffect m_TargetEffect;

        public BaseMeshEffect targetEffect
        {
            get { return m_TargetEffect != this ? m_TargetEffect : null; }
            set
            {
                if (m_TargetEffect == value) return;
                m_TargetEffect = value;

                SetVerticesDirty();
                SetMaterialDirty();
                SetEffectParamsDirty();
            }
        }

        protected override void OnEnable()
        {
            if (targetEffect)
                targetEffect.syncEffects.Add(this);
            base.OnEnable();
        }

        protected override void OnDisable()
        {
            if (targetEffect)
                targetEffect.syncEffects.Remove(this);
            base.OnDisable();
        }

        public override Hash128 GetMaterialHash(Material baseMaterial)
        {
            if (!isActiveAndEnabled) return k_InvalidHash;

            var matEffect = targetEffect as BaseMaterialEffect;
            if (!matEffect || !matEffect.isActiveAndEnabled) return k_InvalidHash;

            return matEffect.GetMaterialHash(baseMaterial);
        }

        public override void ModifyMaterial(Material newMaterial, Graphic graphic)
        {
            if (!isActiveAndEnabled) return;

            var matEffect = targetEffect as BaseMaterialEffect;
            if (!matEffect || !matEffect.isActiveAndEnabled) return;

            matEffect.ModifyMaterial(newMaterial, graphic);
        }

        public override void ModifyMesh(VertexHelper vh, Graphic graphic)
        {
            if (!isActiveAndEnabled) return;
            if (!targetEffect || !targetEffect.isActiveAndEnabled) return;

            targetEffect.ModifyMesh(vh, graphic);
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            SetVerticesDirty();
            SetMaterialDirty();
            SetEffectParamsDirty();
        }
#endif
    }
}
