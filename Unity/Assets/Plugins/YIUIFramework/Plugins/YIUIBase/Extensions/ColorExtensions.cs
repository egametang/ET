using UnityEngine;

namespace YIUIFramework
{
    /// <summary>
    /// <see cref="UnityEngine.Color"/>.
    /// </summary>
    public static class ColorExtensions
    {
        /// <summary>
        /// 设置颜色的alpha值。
        /// </summary>
        public static Color[] SetAlpha(this Color[] colors, float alpha)
        {
            for (int i = 0; i < colors.Length; ++i)
            {
                colors[i].a = alpha;
            }

            return colors;
        }

        /// <summary>
        /// 设置颜色的alpha值。
        /// </summary>
        public static Color SetAlpha(this Color color, float alpha)
        {
            color.a = alpha;
            return color;
        }

        /// <summary>
        /// 将颜色值固定为[0,1]
        /// </summary>
        public static Color Clamp(this Color c)
        {
            for (int i = 0; i < 4; ++i)
            {
                if (float.IsNaN(c[i]) || float.IsNegativeInfinity(c[i]))
                {
                    c[i] = 0.0f;
                }
                else if (float.IsPositiveInfinity(c[i]))
                {
                    c[i] = 1.0f;
                }
                else
                {
                    c[i] = Mathf.Clamp(c[i], 0.0f, 1.0f);
                }
            }

            return c;
        }

        /// <summary>
        /// 计算颜色的亮度。
        /// </summary>
        public static float Luminance(this Color c)
        {
            return (0.3f * c.r) + (0.59f * c.g) + (0.11f * c.b);
        }

        /// <summary>
        /// 只使用颜色中的RGB组件
        /// </summary>
        public static Color LerpRGB(Color a, Color b, float t)
        {
            return new Color(
                Mathf.Lerp(a.r, b.r, t),
                Mathf.Lerp(a.g, b.g, t),
                Mathf.Lerp(a.b, b.b, t),
                a.a);
        }
    }
}