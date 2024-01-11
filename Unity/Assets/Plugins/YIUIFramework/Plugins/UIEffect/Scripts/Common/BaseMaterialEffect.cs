using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Coffee.UIEffects
{
    /// <summary>
    /// Abstract effect base for UI.
    /// </summary>
    [DisallowMultipleComponent]
    public abstract class BaseMaterialEffect : BaseMeshEffect, IParameterTexture, IMaterialModifier
    {
        protected static readonly Hash128 k_InvalidHash = new Hash128();
        protected static readonly List<UIVertex> s_TempVerts = new List<UIVertex>();
        private static readonly StringBuilder s_StringBuilder = new StringBuilder();

        Hash128 _effectMaterialHash;

        /// <summary>
        /// Gets or sets the parameter index.
        /// </summary>
        public int parameterIndex { get; set; }

        /// <summary>
        /// Gets the parameter texture.
        /// </summary>
        public virtual ParameterTexture paramTex
        {
            get { return null; }
        }

        /// <summary>
        /// Mark the vertices as dirty.
        /// </summary>
        public void SetMaterialDirty()
        {
            connector.SetMaterialDirty(graphic);

            foreach (var effect in syncEffects)
            {
                effect.SetMaterialDirty();
            }
        }

        public virtual Hash128 GetMaterialHash(Material baseMaterial)
        {
            return k_InvalidHash;
        }

        public Material GetModifiedMaterial(Material baseMaterial)
        {
            return GetModifiedMaterial(baseMaterial, graphic);
        }

        public virtual Material GetModifiedMaterial(Material baseMaterial, Graphic graphic)
        {
            if (!isActiveAndEnabled) return baseMaterial;

            var oldHash = _effectMaterialHash;
            _effectMaterialHash = GetMaterialHash(baseMaterial);
            var modifiedMaterial = baseMaterial;
            if (_effectMaterialHash.isValid)
            {
                modifiedMaterial = MaterialCache.Register(baseMaterial, _effectMaterialHash, ModifyMaterial, graphic);
            }

            MaterialCache.Unregister(oldHash);

            return modifiedMaterial;
        }

        // protected bool isTMProMobile (Material material)
        // {
        // 	return material && material.shader && material.shader.name.StartsWith ("TextMeshPro/Mobile/", StringComparison.Ordinal);
        // }

        public virtual void ModifyMaterial(Material newMaterial, Graphic graphic)
        {
            if (isActiveAndEnabled && paramTex != null)
                paramTex.RegisterMaterial(newMaterial);
        }

        protected void SetShaderVariants(Material newMaterial, params object[] variants)
        {
            // Set shader keywords as variants
            var keywords = variants.Where(x => 0 < (int) x)
                .Select(x => x.ToString().ToUpper())
                .Concat(newMaterial.shaderKeywords)
                .Distinct()
                .ToArray();
            newMaterial.shaderKeywords = keywords;

            // Add variant name
            s_StringBuilder.Length = 0;
            s_StringBuilder.Append(Path.GetFileName(newMaterial.shader.name));
            foreach (var keyword in keywords)
            {
                s_StringBuilder.Append("-");
                s_StringBuilder.Append(keyword);
            }

            newMaterial.name = s_StringBuilder.ToString();
        }

#if UNITY_EDITOR
        protected override void Reset()
        {
            if (!isActiveAndEnabled) return;
            SetMaterialDirty();
            SetVerticesDirty();
            SetEffectParamsDirty();
        }

        protected override void OnValidate()
        {
            if (!isActiveAndEnabled) return;
            SetVerticesDirty();
            SetEffectParamsDirty();
        }
#endif

        /// <summary>
        /// This function is called when the object becomes enabled and active.
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();

            if (paramTex != null)
            {
                paramTex.Register(this);
            }

            SetMaterialDirty();
            SetEffectParamsDirty();

            // foreach (var mr in GetComponentsInChildren<UIEffectMaterialResolver> ())
            // {
            // 	mr.GetComponent<Graphic> ().SetMaterialDirty ();
            // 	mr.GetComponent<Graphic> ().SetVerticesDirty ();
            // }
        }

        /// <summary>
        /// This function is called when the behaviour becomes disabled () or inactive.
        /// </summary>
        protected override void OnDisable()
        {
            base.OnDisable();

            SetMaterialDirty();

            if (paramTex != null)
            {
                paramTex.Unregister(this);
            }

            MaterialCache.Unregister(_effectMaterialHash);
            _effectMaterialHash = k_InvalidHash;
        }

        // protected override void OnDidApplyAnimationProperties()
        // {
        //     SetEffectParamsDirty();
        // }

        // protected override void OnTextChanged (UnityEngine.Object obj)
        // {
        // 	base.OnTextChanged (obj);
        //
        //
        // 	foreach (var sm in GetComponentsInChildren<TMPro.TMP_SubMeshUI> ())
        // 	{
        // 		if(!sm.GetComponent<UIEffectMaterialResolver>())
        // 		{
        // 			var mr = sm.gameObject.AddComponent<UIEffectMaterialResolver> ();
        //
        // 			targetGraphic.SetAllDirty ();
        // 			//targetGraphic.SetVerticesDirty ();
        //
        // 			//mr.GetComponent<Graphic> ().SetMaterialDirty ();
        // 			//mr.GetComponent<Graphic> ().SetVerticesDirty ();
        //
        //
        // 		}
        // 	}
        // }
    }
}
