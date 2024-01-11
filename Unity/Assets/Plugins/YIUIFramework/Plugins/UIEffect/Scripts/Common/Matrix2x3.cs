using UnityEngine;

namespace Coffee.UIEffects
{
    /// <summary>
    /// Matrix2x3.
    /// </summary>
    public struct Matrix2x3
    {
        public float m00, m01, m02, m10, m11, m12;

        public Matrix2x3(Rect rect, float cos, float sin)
        {
            const float center = 0.5f;
            float dx = -rect.xMin / rect.width - center;
            float dy = -rect.yMin / rect.height - center;
            m00 = cos / rect.width;
            m01 = -sin / rect.height;
            m02 = dx * cos - dy * sin + center;
            m10 = sin / rect.width;
            m11 = cos / rect.height;
            m12 = dx * sin + dy * cos + center;
        }

        public static Vector2 operator *(Matrix2x3 m, Vector2 v)
        {
            return new Vector2(
                (m.m00 * v.x) + (m.m01 * v.y) + m.m02,
                (m.m10 * v.x) + (m.m11 * v.y) + m.m12
            );
        }
    }
}
