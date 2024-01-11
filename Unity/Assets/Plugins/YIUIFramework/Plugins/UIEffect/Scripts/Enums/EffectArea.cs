using UnityEngine;
using UnityEngine.UI;

namespace Coffee.UIEffects
{
    /// <summary>
    /// Area for effect.
    /// </summary>
    public enum EffectArea
    {
        RectTransform,
        Fit,
        Character,
    }

    public static class EffectAreaExtensions
    {
        static readonly Rect rectForCharacter = new Rect(0, 0, 1, 1);
        static readonly Vector2[] splitedCharacterPosition = {Vector2.up, Vector2.one, Vector2.right, Vector2.zero};

        /// <summary>
        /// Gets effect for area.
        /// </summary>
        public static Rect GetEffectArea(this EffectArea area, VertexHelper vh, Rect rectangle, float aspectRatio = -1)
        {
            Rect rect = default(Rect);
            switch (area)
            {
                case EffectArea.RectTransform:
                    rect = rectangle;
                    break;
                case EffectArea.Character:
                    rect = rectForCharacter;
                    break;
                case EffectArea.Fit:
                    // Fit to contents.
                    UIVertex vertex = default(UIVertex);
                    float xMin = float.MaxValue;
                    float yMin = float.MaxValue;
                    float xMax = float.MinValue;
                    float yMax = float.MinValue;
                    for (int i = 0; i < vh.currentVertCount; i++)
                    {
                        vh.PopulateUIVertex(ref vertex, i);
                        float x = vertex.position.x;
                        float y = vertex.position.y;
                        xMin = Mathf.Min(xMin, x);
                        yMin = Mathf.Min(yMin, y);
                        xMax = Mathf.Max(xMax, x);
                        yMax = Mathf.Max(yMax, y);
                    }

                    rect.Set(xMin, yMin, xMax - xMin, yMax - yMin);
                    break;
                default:
                    rect = rectangle;
                    break;
            }


            if (0 < aspectRatio)
            {
                if (rect.width < rect.height)
                {
                    rect.width = rect.height * aspectRatio;
                }
                else
                {
                    rect.height = rect.width / aspectRatio;
                }
            }

            return rect;
        }

        /// <summary>
        /// Gets position factor for area.
        /// </summary>
        public static void GetPositionFactor(this EffectArea area, int index, Rect rect, Vector2 position, bool isText,
            bool isTMPro, out float x, out float y)
        {
            if (isText && area == EffectArea.Character)
            {
                index = isTMPro ? (index + 3) % 4 : index % 4;
                x = splitedCharacterPosition[index].x;
                y = splitedCharacterPosition[index].y;
            }
            else if (area == EffectArea.Fit)
            {
                x = Mathf.Clamp01((position.x - rect.xMin) / rect.width);
                y = Mathf.Clamp01((position.y - rect.yMin) / rect.height);
            }
            else
            {
                x = Mathf.Clamp01(position.x / rect.width + 0.5f);
                y = Mathf.Clamp01(position.y / rect.height + 0.5f);
            }
        }

        /// <summary>
        /// Normalize vertex position by local matrix.
        /// </summary>
        public static void GetNormalizedFactor(this EffectArea area, int index, Matrix2x3 matrix, Vector2 position,
            bool isText, out Vector2 nomalizedPos)
        {
            if (isText && area == EffectArea.Character)
            {
                nomalizedPos = matrix * splitedCharacterPosition[(index + 3) % 4];
            }
            else
            {
                nomalizedPos = matrix * position;
            }
        }
    }
}
