using UnityEngine;

namespace FairyGUI
{
    /// <summary>
    /// 
    /// </summary>
    public class RectHitTest : IHitTest
    {
        /// <summary>
        /// 
        /// </summary>
        public Rect rect;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="contentRect"></param>
        /// <param name="localPoint"></param>
        /// <returns></returns>
        public bool HitTest(Rect contentRect, Vector2 localPoint)
        {
            return rect.Contains(localPoint);
        }
    }
}
