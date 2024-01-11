#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.UI;

namespace YIUIFramework
{
    /// <summary>
    ///自适应不同分辨率。
    /// </summary>
    [RequireComponent(typeof(CanvasScaler))]
    [ExecuteInEditMode]
    public class ResolutionAdapter : MonoBehaviour
    {
        private Canvas       canvas;
        private CanvasScaler scaler;

        private void Awake()
        {
            this.canvas = this.GetComponent<Canvas>();
            if (null == this.canvas || !this.canvas.isRootCanvas)
            {
                return;
            }

            this.AdaptResolution();
        }

        #if UNITY_EDITOR
        private void Update()
        {
            this.AdaptResolution();
        }

        private void OnValidate()
        {
            this.AdaptResolution();
        }
        #endif

        private void AdaptResolution()
        {
            #if UNITY_EDITOR
            var prefabType = PrefabUtility.GetPrefabAssetType(this.gameObject);
            if (prefabType == PrefabAssetType.Regular)
            {
                return;
            }
            #endif

            if (null == this.scaler)
            {
                this.scaler = this.GetComponent<CanvasScaler>();
            }

            var radio    = (float)Screen.width / Screen.height;
            var refRadio = this.scaler.referenceResolution.x / this.scaler.referenceResolution.y;
            if (radio >= refRadio)
            {
                this.scaler.matchWidthOrHeight = 1.0f;
            }
            else
            {
                this.scaler.matchWidthOrHeight = 0.0f;
            }
        }
    }
}