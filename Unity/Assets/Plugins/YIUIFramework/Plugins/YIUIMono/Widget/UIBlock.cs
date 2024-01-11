using UnityEngine;
using UnityEngine.UI;

namespace YIUIFramework
{
    /// <summary>
    /// 不可见的一个图，用来阻挡UI的投射。
    /// </summary>
    public class UIBlock : Graphic, ICanvasRaycastFilter
    {
        public override bool raycastTarget
        {
            get => true;
            set { }
        }

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

        public bool IsRaycastLocationValid(
            Vector2 screenPoint, Camera eventCamera)
        {
            return true;
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
    }
}