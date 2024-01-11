using UnityEngine;
using UnityEngine.UI;

namespace YIUIFramework
{
    [ExecuteInEditMode]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Graphic))]
    public sealed class UIMaterialEffect : MonoBehaviour, IMaterialModifier
    {
        private Graphic             graphic;
        private UIEffectMaterialKey materialKey;
        private Material            material;

        internal UIEffectMaterialKey MaterialKey
        {
            get { return this.materialKey; }

            set
            {
                if (!this.materialKey.Equals(value))
                {
                    this.materialKey = value;
                    if (this.material != null)
                    {
                        UIEffectMaterials.Free(this.material);
                        this.material = null;
                    }
                }
            }
        }

        public Material GetModifiedMaterial(Material baseMaterial)
        {
            Material usedMaterial = baseMaterial;
            if (this.enabled)
            {
                if (this.material == null)
                {
                    this.material = UIEffectMaterials.Get(this.materialKey);
                }

                if (this.material)
                {
                    usedMaterial = this.material;
                }
            }

            var maskable = this.graphic as MaskableGraphic;
            if (maskable != null)
            {
                return maskable.GetModifiedMaterial(usedMaterial);
            }

            return usedMaterial;
        }

        internal void MarkDirty()
        {
            if (this.graphic != null)
            {
                this.graphic.SetMaterialDirty();
            }
        }

        private void Awake()
        {
            this.graphic = this.GetComponent<Graphic>();
        }

        private void OnEnable()
        {
            if (this.graphic != null)
            {
                this.graphic.SetMaterialDirty();
            }
        }

        private void OnDisable()
        {
            if (this.graphic != null)
            {
                this.graphic.SetMaterialDirty();
            }
        }
    }
}