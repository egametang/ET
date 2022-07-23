using UnityEngine;

namespace FairyGUI
{
    /// <summary>
    /// 
    /// </summary>
    public class ColliderHitTest : IHitTest
    {
        /// <summary>
        /// 
        /// </summary>
        public Collider collider;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="contentRect"></param>
        /// <param name="localPoint"></param>
        /// <returns></returns>
        virtual public bool HitTest(Rect contentRect, Vector2 localPoint)
        {
            RaycastHit hit;
            if (!HitTestContext.GetRaycastHitFromCache(HitTestContext.camera, out hit))
                return false;

            if (hit.collider != collider)
                return false;

            return true;
        }
    }
}
