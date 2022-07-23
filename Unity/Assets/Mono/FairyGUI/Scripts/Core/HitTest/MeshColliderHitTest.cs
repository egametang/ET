using UnityEngine;

namespace FairyGUI
{
    /// <summary>
    /// 
    /// </summary>
    public class MeshColliderHitTest : ColliderHitTest
    {
        public Vector2 lastHit;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="collider"></param>
        public MeshColliderHitTest(MeshCollider collider)
        {
            this.collider = collider;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="contentRect"></param>
        /// <param name="localPoint"></param>
        /// <returns></returns>
        override public bool HitTest(Rect contentRect, Vector2 localPoint)
        {
            RaycastHit hit;
            if (!HitTestContext.GetRaycastHitFromCache(HitTestContext.camera, out hit))
                return false;

            if (hit.collider != collider)
                return false;

            lastHit = new Vector2(hit.textureCoord.x * contentRect.width, (1 - hit.textureCoord.y) * contentRect.height);
            HitTestContext.direction = Vector3.back;
            HitTestContext.worldPoint = StageCamera.main.ScreenToWorldPoint(new Vector2(lastHit.x, Screen.height - lastHit.y));

            return true;
        }
    }
}
