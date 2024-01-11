using UnityEngine;
using UnityEngine.UI;

namespace YIUIFramework
{
    /// <summary>
    /// 图像与多边形块
    /// </summary>
    [RequireComponent(typeof(PolygonCollider2D))]
    public sealed class UIBlockPolygon : Graphic, ICanvasRaycastFilter
    {
        public override bool raycastTarget
        {
            get { return true; }
            set { }
        }

        private PolygonCollider2D polygon = null;

        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "StyleCop.CSharp.NamingRules",
            "SA1300:ElementMustBeginWithUpperCaseLetter",
            Justification = "Reviewed. Suppression is OK here.")]
        public override Texture mainTexture
        {
            get { return null; }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "StyleCop.CSharp.NamingRules",
            "SA1300:ElementMustBeginWithUpperCaseLetter",
            Justification = "Reviewed. Suppression is OK here.")]
        public override Material materialForRendering
        {
            get { return null; }
        }

        private PolygonCollider2D Polygon
        {
            get
            {
                if (this.polygon == null)
                {
                    this.polygon = this.GetComponent<PolygonCollider2D>();
                    Physics2D.Simulate(0);
                }

                return this.polygon;
            }
        }

        public bool IsRaycastLocationValid(
            Vector2 screenPoint, Camera eventCamera)
        {
            if (eventCamera != null)
            {
                Vector3 worldPoint;
                if (RectTransformUtility.ScreenPointToWorldPointInRectangle(
                        this.rectTransform,
                        screenPoint,
                        eventCamera,
                        out worldPoint))
                {
                    return this.Polygon.OverlapPoint(worldPoint);
                }

                return false;
            }
            else
            {
                return this.Polygon.OverlapPoint(screenPoint);
            }
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
        }

        public override void SetAllDirty()
        {
        }

        public override void SetLayoutDirty()
        {
        }

        public override void SetVerticesDirty()
        {
        }

        public override void SetMaterialDirty()
        {
        }

        #if UNITY_EDITOR
        protected override void Reset()
        {
            base.Reset();
            this.transform.localPosition = Vector3.zero;
            float w = (this.rectTransform.sizeDelta.x * 0.5f) + 0.1f;
            float h = (this.rectTransform.sizeDelta.y * 0.5f) + 0.1f;
            this.Polygon.points = new Vector2[]
            {
                new Vector2(-w, -h),
                new Vector2(w, -h),
                new Vector2(w, h),
                new Vector2(-w, h)
            };
        }
        #endif
    }
}