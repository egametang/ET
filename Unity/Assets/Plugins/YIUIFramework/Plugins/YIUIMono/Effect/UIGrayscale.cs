#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace YIUIFramework
{
    [ExecuteInEditMode]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(UIMaterialEffect))]
    public sealed class UIGrayscale : MonoBehaviour
    {
        [SerializeField]
        [Range(0, 255)]
        private int grayscale = 0;

        private UIMaterialEffect materialEffect;

        public int GrayScale
        {
            get { return this.grayscale; }

            set
            {
                if (this.grayscale != value)
                {
                    this.grayscale = value;
                    this.Refresh();
                }
            }
        }

        private void Awake()
        {
            this.Refresh();
        }

        private void OnDestroy()
        {
            if (this.materialEffect != null)
            {
                var key = this.materialEffect.MaterialKey;
                key.GrayScale                   = 0;
                this.materialEffect.MaterialKey = key;
                this.materialEffect.MarkDirty();
            }
        }

        #if UNITY_EDITOR
        private void OnValidate()
        {
            this.Refresh();
        }
        #endif

        private void Refresh()
        {
            #if UNITY_EDITOR
            var prefabType = PrefabUtility.GetPrefabType(this.gameObject);
            if (prefabType == PrefabType.Prefab)
            {
                return;
            }
            #endif

            if (this.materialEffect == null)
            {
                this.materialEffect =
                    this.GetOrAddComponent<UIMaterialEffect>();
            }

            var key = this.materialEffect.MaterialKey;
            key.GrayScale                   = (byte)this.grayscale;
            this.materialEffect.MaterialKey = key;
            this.materialEffect.MarkDirty();
        }
    }
}