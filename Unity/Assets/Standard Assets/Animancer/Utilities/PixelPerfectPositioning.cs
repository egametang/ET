// Animancer // https://kybernetik.com.au/animancer // Copyright 2020 Kybernetik //

using UnityEngine;

namespace Animancer
{
    /// <summary>
    /// Adjusts the <see cref="Transform.localPosition"/> every frame to keep this object aligned to a grid with a size
    /// determined by the <see cref="Renderer"/> while wrapping the value to keep it as close to 0 as possible.
    /// </summary>
    /// <example><see href="https://kybernetik.com.au/animancer/docs/examples/directional-sprites/character-controller">Character Controller</see></example>
    /// https://kybernetik.com.au/animancer/api/Animancer/PixelPerfectPositioning
    /// 
    [AddComponentMenu(Strings.MenuPrefix + "Pixel Perfect Positioning")]
    [HelpURL(Strings.DocsURLs.APIDocumentation + "/" + nameof(PixelPerfectPositioning))]
    public sealed class PixelPerfectPositioning : MonoBehaviour
    {
        /************************************************************************************************************************/

        [SerializeField]
        private SpriteRenderer _Renderer;

        /// <summary>[<see cref="SerializeField"/>]
        /// The <see cref="SpriteRenderer"/> that will have its position adjusted.
        /// </summary>
        public ref SpriteRenderer Renderer => ref _Renderer;

        /************************************************************************************************************************/

#if UNITY_EDITOR
        private void Reset()
        {
            _Renderer = Editor.AnimancerEditorUtilities.GetComponentInHierarchy<SpriteRenderer>(gameObject);
        }
#endif

        /************************************************************************************************************************/

        private void Update()
        {
            var transform = _Renderer.transform;
            var position = transform.position;

            // Snap the position to the pixel grid.
            var pixelsPerUnit = _Renderer.sprite.pixelsPerUnit;
            transform.position = new Vector3(
                Mathf.Round(position.x / pixelsPerUnit) * pixelsPerUnit,
                Mathf.Round(position.y / pixelsPerUnit) * pixelsPerUnit,
                Mathf.Round(position.z / pixelsPerUnit) * pixelsPerUnit);

            // Keep the local position as small as possible while staying on the grid.
            var maxLocalPosition = 0.5f / pixelsPerUnit;
            position = transform.localPosition;
            WrapValue(ref position.x, maxLocalPosition);
            WrapValue(ref position.y, maxLocalPosition);
            WrapValue(ref position.z, maxLocalPosition);
            transform.localPosition = position;
        }

        /************************************************************************************************************************/

        private void WrapValue(ref float value, float max)
        {
            value %= max * 2;

            if (value > max) value -= max * 2;
            else if (value < -max) value += max * 2;
        }

        /************************************************************************************************************************/
    }
}
