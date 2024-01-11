using UnityEngine;

namespace YIUIFramework
{

    /// <summary>
    /// <see cref="RectTransform"/>.
    /// </summary>
    public static class RectTransformExtensions
    {
        private static Vector3[] corners = new Vector3[4];

        /// <summary>
        /// 获取世界中心位置
        /// </summary>
        public static Vector3 GetWorldCenter(this RectTransform transform)
        {
            transform.GetWorldCorners(corners);
            return (corners[0] + corners[2]) / 2;
        }

        /// <summary>
        /// 获取世界中心位置 X
        /// </summary>
        public static float GetWorldCenterX(this RectTransform transform)
        {
            transform.GetWorldCorners(corners);
            return (corners[0].x + corners[2].x) / 2;
        }

        /// <summary>
        /// 获取世界中心位置 Y
        /// </summary>
        public static float GetWorldCenterY(this RectTransform transform)
        {
            transform.GetWorldCorners(corners);
            return (corners[0].y + corners[2].y) / 2;
        }

        /// <summary>
        /// 获取世界中心位置 Z
        /// </summary>
        public static float GetWorldCenterZ(this RectTransform transform)
        {
            transform.GetWorldCorners(corners);
            return (corners[0].z + corners[2].z) / 2;
        }
    }
}
